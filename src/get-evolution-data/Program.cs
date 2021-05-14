using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;

namespace get_evolution_data
{
    class Program
    {
        public class OutputFormat
        {
            public Dictionary<string, string> evolutionFamilyIdByUniqueId { get; set; }
            public Dictionary<string, List<string>> uniqueIdsForFamilyId { get; set; }
        }

        static void Main(string[] args)
        {
            var resultsByUniqueId = new Dictionary<string, string>();
            var resultsByFamilyId = new Dictionary<string, List<string>>();

            using(var client = new HttpClient())
            {
                using (var inputStream = client.GetStreamAsync("https://raw.githubusercontent.com/pokemongo-dev-contrib/pokemongo-game-master/master/versions/latest/GAME_MASTER.json").Result)
                using (var inputStreamReader = new StreamReader(inputStream))
                using (var inputJsonReader = new JsonTextReader(inputStreamReader))
                {
                    var serializer = new JsonSerializer();

                    var allPokemon = serializer.Deserialize<GameMasterRoot>(inputJsonReader);

                    foreach(var poke in allPokemon.itemTemplate.Where(item => item.pokemon != null))
                    {
                        var uniqueId = poke.pokemon.uniqueId.ToLower();
                        var familyId = poke.pokemon.familyId.ToLower();

                        resultsByUniqueId[uniqueId] = familyId;

                        if (!resultsByFamilyId.ContainsKey(familyId))
                            resultsByFamilyId[familyId] = new List<string>();

                        if (!resultsByFamilyId[familyId].Contains(uniqueId))
                            resultsByFamilyId[familyId].Add(uniqueId);
                    }
                }
            }

            File.WriteAllText("./pokemon-eveolution-data.json",
                JsonConvert.SerializeObject(new OutputFormat()
                {
                    evolutionFamilyIdByUniqueId = resultsByUniqueId,
                    uniqueIdsForFamilyId = resultsByFamilyId,
                }, Formatting.Indented),
                new UTF8Encoding(false));
        }
    }

    public class GameMasterRoot
    {
        public GameMasterEntry[] itemTemplate { get; set; }
    }

    public class GameMasterEntry
    {
        public string templateId { get; set; }
        public GameMasterEntryPokemon pokemon { get; set; }
    }

    public class GameMasterEntryPokemon
    {
        public string uniqueId { get; set; }
        public string familyId { get; set; }
    }
}
