### MLOps.NET
MLOps.NET is an SDK tool to track and manage lifecycle of an ML.NET machine learning model.

![.NET Core](https://github.com/aslotte/MLOps.NET/workflows/.NET%20Core/badge.svg)

#### Tell me more
If you're used to creating machine learning models in libraries such as Tensorflow, Keras, Scikitlearn or H2O you may have come across libraries such as MLflow or Neptune to manage and track the life-cycle of your machine learning models. 

Models created in ML.NET can however currently not be used in MLflow, and as such the idea of MLOps.NET was spawn.

#### Currently supported storage providers

##### Model Repository
- Azure Blob storage

##### Metadata storage
- Azure TableStorage (CosmosDB)
- SQLite

#### Roadmap
- Add Blazor WebAssembly client to vizualize model training
- Add support for SQL Server
- Add support for file shares
- Add support additional support for tracking a models performance
- Add model deployment support
- Add data tracking support

#### Getting started
An alpha version of the NuGet package will be released shortly, stay tuned
