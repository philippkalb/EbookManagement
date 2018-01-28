# What am I?
This is a very trivial ebook library management tool. It crawls a folder that contains epub ebooks and adds meta information of each book into elastic search.
With elastic search this tool is able to provide searching for your potentially huge library

## Configuration

Check appsettings to define the folder that contains the library and the connection string for elastic search
  "Basefolder": "D:\\LibraryFolder\\",
  "ElasticSearchConnection": "http://localhost:9200"

## Elastic setup:

Indexname: booksearch
Mapping: booksearch

```javascript
 {
    "settings": {
        "analysis": {
            "analyzer": {
                "namegrams": {
                    "type": "custom",
                    "tokenizer": "keyword",
                    "filter": [
                        "ngrams_filter"
                    ]
                }
            },
            "filter": {
                "ngrams_filter": {
                    "type": "ngram",
                    "min_gram": 3,
                    "max_gram": 10
                }
            }
        }
    },
	 "mappings" : {
    "book": {
        "properties": {
            "title": {
                "type": "text",
                "analyzer": "namegrams"
            },
				 		"author": {
                "type": "text",
                "analyzer": "namegrams"
            }
				  
        }
    }
} 	 
}
```

## Licence

Do what you want with this code...