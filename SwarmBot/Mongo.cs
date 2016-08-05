using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Driver;
using MongoDB.Bson;
using Trileans;

namespace SwarmBot.MongoDB
{
    public class Mongo
    {
        public IMongoClient client { get; }

        public Mongo(string address)
        {
            client = new MongoClient(address);
        }
        public MDatabase GetDatabase(string dbName, string[] instantiateWithCollections)
        {
            return new MDatabase(dbName, instantiateWithCollections, client);
        }
    }
    public class MDatabase
    {
        public IMongoDatabase database { get; }
        public IMongoCollection<BsonDocument>[] collections { get; }

        public MDatabase(string dbName, string[] instantiateWithCollections, IMongoClient client)
        {
            database = client.GetDatabase(dbName);
            collections = new IMongoCollection<BsonDocument>[instantiateWithCollections.Length];
            for (var i = 0; i < instantiateWithCollections.Length; i++)
            {
                collections[i] = database.GetCollection<BsonDocument>(instantiateWithCollections[i]);
            }
        }

        public trilean getBsonDocumentByPropertyEq(string propertyName, string propertyValue, IMongoCollection<BsonDocument> collection)
        {
            FilterDefinition<BsonDocument> filter = Builders<BsonDocument>.Filter.Regex(propertyName, new BsonRegularExpression(propertyValue, "i"));
            List<BsonDocument> list = collection.Find(filter).ToList();
            if(list.Count >= 1)
            {
                return new trilean(true, list[0].ToJson());
            } else
            {
                return new trilean(false);
            }
        }
    }
}
