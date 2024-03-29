﻿//-----------------------------------------------------------------------
// <copyright file="JsonDocumentModel.cs" company="Visual JSON Editor">
//     Copyright (c) Rico Suter. All rights reserved.
// </copyright>
// <license>http://visualjsoneditor.codeplex.com/license</license>
// <author>Rico Suter, mail@rsuter.com</author>
//-----------------------------------------------------------------------

using System.IO;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using NJsonSchema;

namespace ExcelPatternTool.Model.JsonEditor
{
    /// <summary>Represents a JSON document. </summary>
    public class JsonDocumentModel : ObservableObject
    {
        private string _filePath;
        private JsonObjectModel _data;
        private bool _isReadOnly;

        /// <summary>Initializes a new instance of the <see cref="JsonDocumentModel"/> class. </summary>
        protected JsonDocumentModel() { }

        /// <summary>Gets or sets the document's file path. </summary>
        public string FilePath
        {
            get { return _filePath; }
            set
            {
                _filePath = value;

                OnPropertyChanged(nameof(HasFileLocation));
                OnPropertyChanged(nameof(DisplayTitle));

            }
        }

        /// <summary>Gets the path of the schema file. </summary>
        public string SchemaPath { get; private set; }

        /// <summary>Gets the document's display title. </summary>
        public string DisplayTitle
        {
            get
            {
                if (HasFileLocation)
                    return Path.GetFileName(FilePath);
                return "Unsaved";
            }
        }

        /// <summary>Gets a value indicating whether the document has a file location. </summary>
        public bool HasFileLocation
        {
            get { return _filePath != null; }
        }

        /// <summary>Gets the JSON data. </summary>
        public JsonObjectModel Data { get; set; }


        /// <summary>Gets or sets the undo/redo manager. </summary>

        /// <summary>Gets a value indicating whether the document is read only. </summary>
        public bool IsReadOnly { get; set; }


        /// <summary>Initializes the document. </summary>
        /// <param name="data">The JSON data. </param>
        /// <param name="dispatcher">The UI dispatcher. </param>
        public void Initialize(JsonObjectModel data)
        {
            Data = data;
        }

        /// <summary>Generates the default schema file path for a given JSON file path. </summary>
        /// <param name="filePath">The JSON document file path. </param>
        /// <returns>The path to the schema file. </returns>
        public static string GetDefaultSchemaPath(string filePath)
        {
            var directoryName = Path.GetDirectoryName(filePath);
            if (string.IsNullOrEmpty(directoryName))
                return Path.GetFileNameWithoutExtension(filePath) + ".schema" + Path.GetExtension(filePath);
            return Path.Combine(directoryName, Path.GetFileNameWithoutExtension(filePath) + ".schema" + Path.GetExtension(filePath));
        }

        /// <summary>Loads a <see cref="JsonDocumentModel"/> from a file path. The schema path is automatically determined. </summary>
        /// <param name="filePath">The file path. </param>
        /// <param name="dispatcher">The UI dispatcher. </param>
        /// <returns>The <see cref="JsonDocumentModel"/>. </returns>
        public static Task<JsonDocumentModel> LoadAsync(string filePath)
        {
            var schemaPath = GetDefaultSchemaPath(filePath);
            return LoadAsync(filePath, schemaPath);
        }

        /// <summary>Loads a <see cref="JsonDocumentModel"/> from a file path and schema path. </summary>
        /// <param name="filePath">The file path. </param>
        /// <param name="schemaPath">The schema path. </param>
        /// <param name="dispatcher">The UI dispatcher. </param>
        /// <returns>The <see cref="JsonDocumentModel"/>. </returns>
        public static Task<JsonDocumentModel> LoadAsync(string filePath, string schemaPath)
        {
            return Task.Run(() =>
            {
                var schema = JsonSchema.FromFileAsync(schemaPath).GetAwaiter().GetResult();
                var data = JsonObjectModel.FromJson(File.ReadAllText(filePath, Encoding.UTF8), schema);

                var document = new JsonDocumentModel();
                document.Initialize(data);
                document.FilePath = filePath;
                document.SchemaPath = schemaPath;
                return document;
            });
        }

        /// <summary>Creates a new <see cref="JsonDocumentModel"/> based on a given schema file path. </summary>
        /// <param name="schemaPath">The schema file path. </param>
        /// <param name="dispatcher">The UI dispatcher. </param>
        /// <returns>The <see cref="JsonDocumentModel"/>. </returns>
        public static Task<JsonDocumentModel> CreateAsync(string schemaPath)
        {
            return Task.Run(() =>
            {
                var schema = JsonSchema.FromFileAsync(schemaPath).GetAwaiter().GetResult();
                var data = JsonObjectModel.FromSchema(schema);

                var document = new JsonDocumentModel();
                document.Initialize(data);
                return document;
            });
        }

        /// <summary>Saves the document. </summary>
        /// <param name="saveSchema">Defines if the schema file should be saved too. </param>
        /// <returns>The task. </returns>
        /// <exception cref="IOException">The document has no file location</exception>
        public async Task SaveAsync(bool saveSchema)
        {
            if (!HasFileLocation)
                throw new IOException("The document has no file location");

            await Task.Run(() =>
            {
                var jsonData = Data.ToJson();
                File.WriteAllText(FilePath, jsonData, Encoding.UTF8);

                if (saveSchema)
                {
                    var schemaPath = GetDefaultSchemaPath(FilePath);
                    File.WriteAllText(schemaPath, Data.Schema.ToJson());
                }
            });
        }

        /// <summary>Returns a string that represents the current object. </summary>
        /// <returns>A string that represents the current object. </returns>
        public override string ToString()
        {
            if (HasFileLocation)
                return Path.GetFileName(FilePath);
            return "Unsaved";
        }
    }
}
