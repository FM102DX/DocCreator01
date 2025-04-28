using DocCreator01.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocCreator01.Contracts
{ 
    public interface IProjectService
    {
        /// <summary>
        /// Текущий загруженный проект.
        /// </summary>
        Project CurrentProject { get; }

        /// <summary>
        /// Загрузить проект из JSON-файла по указанному пути.
        /// При невалидном формате выбрасывает JsonException.
        /// </summary>
        /// <param name="path">Путь к файлу проекта (.json).</param>
        void Load(string path);

        /// <summary>
        /// Сохранить текущий проект в JSON-файл по указанному пути.
        /// </summary>
        /// <param name="path">Путь для сохранения файла проекта (.json).</param>
        void Save(string path);
    }
}