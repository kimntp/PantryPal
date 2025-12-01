using System.Text.Json;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Maui.Storage;

namespace PPApp.Services {
    public class RecipeSearchService {
        
       public RecipeSearchService() {
            try {
                using Stream fileStream = FileSystem.Current.OpenAppPackageFileAsync("cleanedDataset.json").Result;
                using StreamReader reader = new StreamReader(fileStream);
                string contents = reader.ReadToEndAsync().Result;
                
            }
            string jsonString = File.ReadAllText();
        }
    }
}
