# website-cacher
Simple utility for downloading web pages for offline browsing.

Application has two parts. Server written in C# .NET Core 3.1 and a web client in Typescript and Node.js. The purpose of the server is to download, scrape and cache webpages which are then server over http client. The server has no user interface. The purpose of the Webclient is to controll the behaviour of the server and give a youser some information when he/she browses the webpages.

## Web client
Web client is located in `WebClient` directory. For now, Web client is very simple just to demonstrate the functionality of the server. Build instructions are:
```PowerShell
cd WebClient
npm install
npm run build
```

Then you have to copy all the files from `WebClient/dist` and `WebClient/static` directories into `static/` directory where the server is located. These files will be hosted by a WebsiteCacher to a client with offline webpages.
```PowerShell
cp WebClient/dist/*
```

## Server
As mentioned above, the server is written in C# .NET Core 3.1 and uses Entity Framework Core. There are following entities:

- **Resource** represents single file which was downloaded from the internet. Each resource can be accessed by a web browser.
- **PageQuery** represents a request for downloading bunch of websites from the internet. Each query has a starting url address, required depth and regular expressions describing which links to follow.
- **Page** represents a webpage under the specific `PageQuery`. Pages creates a tree with PageQuery as a root. Each page has link to its `Resource`, list of `Page`s which are linked from the page and list of `Resource`s representing media which belong to the page.

There can be multiple `Page` entities with same url because they can belong to different `PageQuery`.

Each of these three entities is a wrapper for `ResourceData`, `PageQueryData` and `PageData` respectively, which holds data which are stored in the database. Downloaded resources are stored in a folder next to the application file.

When you remove `PageQuery` or its content is changed, it may happen that some pages will no longer be linked to the original tree. To remove them, you need to run cleanup method which first removes all `Page`s which are not linked and then all `Resource` which are also not linked.


### Database
To initialize database please run the following commands in Package Manager Console (PMC)

```PowerShell
Add-Migration WebsiteCacherDatabase
Update-Database
```
