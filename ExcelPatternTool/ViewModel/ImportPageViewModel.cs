using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using AutoMapper;
using CommunityToolkit.Mvvm.Input;
using ExcelPatternTool.Core.DataBase;
using ExcelPatternTool.Core.Validators;
using ExcelPatternTool.Model;
using ExcelPatternTool.Core.Helper;
using ExcelPatternTool.Core;
using ExcelPatternTool.Model.Dto;
using CommunityToolkit.Mvvm.DependencyInjection;
using ExcelPatternTool.Core.Validators.Implements;
using ExcelPatternTool.Helper;
using ExcelPatternTool.Core.Excel.Models;
using ExcelPatternTool.Core.Excel.Models.Interfaces;
using ExcelPatternTool.Core.EntityProxy;
using ExcelPatternTool.Core.Patterns;
using ExcelPatternTool.Common;
using System.Threading.Tasks;
using MahApps.Metro.Controls.Dialogs;
using MahApps.Metro.Controls;
using Microsoft.EntityFrameworkCore;

namespace ExcelPatternTool.ViewModel
{
    public class ImportPageViewModel : ObservableObject
    {
        public event EventHandler OnFinished;
        private Validator validator;
        private Pattern _pattern;
        public ImportPageViewModel(DbContextFactory dbContextFactory)
        {
            validator = Ioc.Default.GetRequiredService<Validator>();
            validator.SetValidatorProvider(EntityProxyContainer.Current.EntityType, new DefaultValidatorProvider());
            this.ValidDataCommand = new RelayCommand(GetDataAction, CanValidate);
            this.SubmitCommand = new RelayCommand(SubmitAction, CanSubmit);
            this.Entities = new ObservableCollection<object>();
            this.ProcessResultList = new ObservableCollection<ProcessResultDto>();
            this.ProcessResultList.CollectionChanged += ProcessResultList_CollectionChanged;
            this.PropertyChanged += ImportPageViewModel_PropertyChanged;
            this.dbContextFactory=dbContextFactory;
        }

        private void ImportPageViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(this.IsValidSuccess))
            {
                SubmitCommand.NotifyCanExecuteChanged();
            }
            else if (e.PropertyName == nameof(this.Entities))
            {
                SubmitCommand.NotifyCanExecuteChanged();
                ValidDataCommand.NotifyCanExecuteChanged();

            }
        }

        private void ProcessResultList_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            this.IsValidSuccess = this.ProcessResultList.Count == 0;
        }

        private async void SubmitAction()
        {
            var task = InvokeHelper.InvokeOnUi<IEnumerable<object>>(null, () =>
            {

                foreach (var employee in Entities)
                {
                    Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        Ioc.Default.GetRequiredService<CategoryPageViewModel>().Entities.Add((IExcelEntity)employee);
                    });

                }


                return Entities;



            }, async (t) =>
            {

                this.Entities.Clear();
                this.OnFinished?.Invoke(this, EventArgs.Empty);
                MessageBox.Show("已完成导入");

            });
        }

        private void GetDataAction()
        {
            this.ProcessResultList.Clear();
            foreach (var item in this.Entities)
            {

                var row = (item as IExcelEntity).RowNumber + 1;
                var id = ProcessResultList.Count + 1;
                var level = 1;


                var validateResult = validator.Validate(item);
                var result = validateResult.Where(c => c.IsValidated == false)
                    .Select(c => new ProcessResultDto()
                    {
                        Id = id,
                        Row = row,
                        Column = c.Column,
                        Level = level,
                        Content = c.Content,
                        KeyName = c.KeyName,
                    });


                foreach (var processResultDto in result)
                {
                    this.ProcessResultList.Add(processResultDto);

                }


            }
            var currentCount = ProcessResultList.Count();

        }




        private async void ImportFromSqliteAction()
        {
            await ImportFromDb("sqlite");
        }


        private async void ImportFromSqlServerAction()
        {
            await ImportFromDb("sqlserver");
        }

        private async void ImportFromMySqlAction()
        {
            await ImportFromDb("mysql");
        }

        private async Task ImportFromDb(string dbtype)
        {
            _pattern = LocalDataHelper.ReadObjectLocal<Pattern>();

            this.Entities.Clear();
            var result = await DialogManager.ShowInputAsync((MetroWindow)App.Current.MainWindow, "从数据库导入", "请填写数据库连接字符串");
            if (string.IsNullOrEmpty(result))
            {
                return;
            }
            var task = InvokeHelper.InvokeOnUi<IEnumerable<object>>(null, () =>
            {
                using (var dbcontext = dbContextFactory.CreateExcelPatternToolDbContext(result, dbtype))
                {
                    var dbset = dbcontext.GetDbSet(EntityProxyContainer.Current.EntityType);
                    return (dbset as IEnumerable<object>).ToList();
                }

            }, (t) =>
            {
                var data = t;
                if (data != null)
                {
                    this.Entities = new ObservableCollection<object>(data);
                    this.IsValidSuccess = null;
                }
            });

        }

        private void ImportFromExcelAction()
        {
            _pattern = LocalDataHelper.ReadObjectLocal<Pattern>();

            this.Entities.Clear();
            var task = InvokeHelper.InvokeOnUi<dynamic>(null, () =>
            {
                var result = DocHelper.ImportFromDelegator((importer) =>
                {

                    var op1 = new ImportOption(EntityProxyContainer.Current.EntityType, _pattern.ExcelImport.SheetNumber, _pattern.ExcelImport.SkipRow);
                    op1.SheetName=_pattern.ExcelImport.SheetName;
                    var r1 = importer.Process(EntityProxyContainer.Current.EntityType, op1);

                    return new { Employees = r1 };

                });
                return result;


            }, (t) =>
            {
                var data = t;
                if (data != null)
                {


                    this.Entities = new ObservableCollection<object>(data.Employees);
                    this.IsValidSuccess = null;
                }
            });

        }


        private ObservableCollection<ProcessResultDto> _processResultList;

        public ObservableCollection<ProcessResultDto> ProcessResultList
        {
            get { return _processResultList; }
            set
            {
                _processResultList = value;
                OnPropertyChanged(nameof(ProcessResultList));
            }
        }
        private ObservableCollection<object> _employees;

        public ObservableCollection<object> Entities
        {
            get { return _employees; }
            set
            {
                _employees = value;
                OnPropertyChanged(nameof(Entities));
            }
        }


        private bool? _isValidSuccess;
        private readonly DbContextFactory dbContextFactory;

        public bool? IsValidSuccess
        {
            get { return _isValidSuccess; }
            set
            {
                _isValidSuccess = value;

                OnPropertyChanged();
            }
        }

        private bool CanSubmit()
        {
            return IsValidSuccess.HasValue && IsValidSuccess.Value;
        }

        private bool CanValidate()
        {
            if (this.Entities.Count > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public List<MenuCommand> ImportOptions => new List<MenuCommand>() {
            new MenuCommand("从Excel导入", ImportFromExcelAction, () => true),
            new MenuCommand("从SqlServer导入", ImportFromSqlServerAction, () => true),
            new MenuCommand("从Sqlite导入", ImportFromSqliteAction, () => true),
            new MenuCommand("从MySql导入", ImportFromMySqlAction, () => true),
        };


        public RelayCommand ValidDataCommand { get; set; }
        public RelayCommand SubmitCommand { get; set; }

    }
}
