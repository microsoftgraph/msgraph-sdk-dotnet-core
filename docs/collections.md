Collections in the Microsoft Graph .NET Client Library
=====

Whenever a request returns a collection of objects that allow paging or navigation into the collection, the library generates a complex collection page object.

## Getting a collection

To retrieve a collection, like the list of groups in the service, you call `GetAsync` on the collection request:

```csharp
await graphServiceClient
	    .Groups
	    .Request()
	    .GetAsync();
```

`GetAsync` returns an `ICollectionPage<T>` implementation on success and throws a `ServiceException` on error. For the groups collection, the type returned is `IGraphServiceGroupsCollectionPage`, which inherits `ICollectionPage<Item>`.

`IGraphServiceGroupsCollectionPage` contains three properties: 

|Name                |Description                                                                                                                                                  |
|--------------------|-------------------------------------------------------------------------------------------------------------------------------------------------------------|
|**CurrentPage**     |An `IList<Item>`.                                                                                                                                            |
|**NextPageRequest** |An `IGraphServiceGroupsCollectionRequest` used to get to the next page of items, if another page exists. This value will be null if there is not a next page.|
|**AdditionalData**    |An `IDictionary<string, object>` to any additional values returned by the service. In this case, none.                                                       |

## Adding to a collection

Some collections, like the groups collection, can be changed. To create a group you can call:

```csharp
var groupToCreate = new Group
    {
		GroupTypes = new List<string> { "Unified" },
		DisplayName = "Unified group",
		Description = "Best group ever",
		...
	};
	
var newGroup = await graphServiceClient
                         .Groups
						 .Request()
						 .AddAsync(groupToCreate);
```

`AddAsync` returns the created group on success and throws a `ServiceException` on error.

## Expanding a collection

To expand a collection, you call `Expand` on the collection request object with the string value of the expand:

```csharp
var children = await graphServiceClient
                         .Me
                         .Drive
						 .Items[itemId]
						 .Children
						 .Request()
						 .Expand("thumbnails")
						 .GetAsync();
```
