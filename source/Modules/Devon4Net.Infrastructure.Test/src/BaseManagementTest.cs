using System;
using System.IO;
using System.Linq;
using AutoMapper;
using Devon4Net.Infrastructure.Extensions;

namespace Devon4Net.Infrastructure.Test
{
    public abstract class BaseManagementTest :IDisposable
    {
        public IMapper Mapper { get; set; }        
        public abstract void ConfigureMapper();
        

        protected BaseManagementTest()
        {
            ConfigureMapper();
        }

        protected string GetFilePath(string fileName)
        {
            //var basePath = $"{System.Reflection.Assembly.GetExecutingAssembly().CodeBase.GetDirectoryFromString()}/";//PlatformServices.Default.Application.ApplicationBasePath;
            var basePath = $"{System.Reflection.Assembly.GetExecutingAssembly().Location}/";
            return Directory.GetFiles(basePath, fileName, SearchOption.AllDirectories).ToList().FirstOrDefault();
        }

        public void Dispose()
        {            
        }
    }
}