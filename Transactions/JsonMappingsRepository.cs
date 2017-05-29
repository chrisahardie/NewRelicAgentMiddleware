using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace NewRelicAgentMiddleware.Transactions
{
    /// <summary>
    /// Retrieves mappings from JSON file
    /// </summary>
    internal class JsonMappingsRepository : IMappingsRepository
    {
        private IEnumerable<ActionMapping> mappings;
        private string mappingsFileDirectory = Path.Combine($"{AppContext.BaseDirectory}", "Mappings");
        private string mappingsFileName = "mappings.json";
        private FileSystemWatcher fileSystemWatcher;
        private Action<IEnumerable<ActionMapping>> callback;
        private readonly ILogger logger;
        private const int errorEventId = 1001;

        public JsonMappingsRepository()
        {
            logger = new LoggerFactory().CreateLogger<JsonMappingsRepository>();

            fileSystemWatcher = new FileSystemWatcher();
            fileSystemWatcher.Path = mappingsFileDirectory;
            fileSystemWatcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size | NotifyFilters.CreationTime;
            fileSystemWatcher.Filter = mappingsFileName;
            fileSystemWatcher.Changed += OnFileChangedOrCreated;
            fileSystemWatcher.Created += OnFileChangedOrCreated;
            fileSystemWatcher.EnableRaisingEvents = true;
        }

        public IEnumerable<ActionMapping> Get()
        {
            mappings = new List<ActionMapping>();
            try
            {
                var path = GetPathToMappingJson();
                var json = File.ReadAllText(path);
                mappings = JsonConvert.DeserializeObject<IEnumerable<ActionMapping>>(json);
            }
            catch (FileNotFoundException exc)
            {
                logger.LogError(errorEventId, exc, "Could not find mappings JSON document for New Relic middleware SDK");
            }
            catch (JsonException exc)
            {
                logger.LogError(errorEventId, exc, "Error deserializing New Relic middleware JSON document");
            }
            catch (Exception exc)
            {
                logger.LogError(errorEventId, exc, "Unknown error while trying to read New Relic middleware JSON document");
            }

            return mappings;
        }

        public void RegisterCallbackOnUpdate(Action<IEnumerable<ActionMapping>> callback)
        {
            this.callback = callback;
        }

        private void OnFileChangedOrCreated(object sender, FileSystemEventArgs e)
        {
            callback(Get());
        }

        /// <summary>
        /// Probably want to implement this better when we figure out how we're managing the JSON...
        /// </summary>
        /// <returns>Filesystem path to json mapping document</returns>
        private string GetPathToMappingJson()
        {
            return Path.Combine(mappingsFileDirectory, mappingsFileName);
        }
    }
}
