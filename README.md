# Query

This is a simple library that is used to run MySql Commands (and SQL ... still in Testing).
It uses reflection in order to map the results to the specified Data type.
### Usage:
###### In the examples below `connection` can be either `MySqlConnection` or `SqlConnection`
##### Simple usage to return a `DbDataReader` 
```C#
DbDataReader result = Query.Create("SELECT Col_1, Col_2 FROM tbl")
                .WithConnection(connection).ExecuteReader();
```
##### Simple usage to return a `DbDataReader`  with passing in parameters
```C#
DbDataReader result = Query.Create("SELECT Col_1, Col_2 FROM tbl WHERE Col_3 = @Col_3")
                .WithConnection(connection).WithParameters(new { Col_3 = "Value" }).ExecuteReader();
```
###### `WithParameters(object parameters)` takes in an [Anonymous Type](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/classes-and-structs/anonymous-types)
##### Simple usage to return an `IEnumerable<MyClass>`
```C#
IEnumerable<MyClass> result = Query.Create("SELECT FirstName, LastName, Age FROM tbl")
                .WithConnection(connection).ExecuteReader<MyClass>();
```

```C#
public class MyClass
	{
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int Age { get; set; }
    }
```
###### `ExecuteReader<T>` maps the columns in the results to the properties (**Not Fields**) of the passed in class `<T>`
##### Simple usage to return an `IEnumerable<string>`
```C#
IEnumerable<string> result = Query.Create("SELECT FirstName FROM tbl")
                .WithConnection(connection).IsPrimitive().ExecuteReader<string>();
```
###### `IsPrimitive()` is used when the passed in data type is primitive (`string`,`int`,`double`,`bool`...)
##### Simple usage to return an `string`
```C#
string result = Query.Create("SELECT FirstName FROM tbl")
                .WithConnection(connection).ExecuteScalar<string>();
```
###### `Default(object Default)` can be called before `ExecuteScalar<T>` to set a value that will be returned if the `ExecuteScalar` returns no value
##### Simple usage execute a non query
```C#
int result = Query.Create("UPDATE tbl SET FirstName = @FirstName").
                .WithConnection(connection)WithParameters(new { FirstName = "Matthew" }).ExecuteNonQuery();
```
##### Simple usage to return a `DataTable`
```C#
DataTable result = Query.Create("SELECT FirstName, LastName, Age FROM tbl")
                .WithConnection(connection).ReaderIntoDataTable();
```
###### `ReaderIntoDataTable()` populates a `DataTable` with the results of the `ExecuteReader()`

The method `Lazy()`  enables [Lazy Initialization](https://docs.microsoft.com/en-us/dotnet/framework/performance/lazy-initialization) on the `ExecuteReader()`
