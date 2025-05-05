using DocCreator01.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocCreator01.Contracts
{
    public interface IProjectRepository
    {
        /// <summary>
        /// Loads a project from the specified path
        /// </summary>
        /// <param name="path">The path to load the project from</param>
        /// <returns>The loaded project</returns>
        Project Load(string path);
        
        /// <summary>
        /// Saves a project to the specified path
        /// </summary>
        /// <param name="project">The project to save</param>
        /// <param name="path">The path to save the project to</param>
        void Save(Project project, string path);
    }
}
