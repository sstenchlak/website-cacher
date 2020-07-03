# website-cacher
Simple utility for downloading web pages for offline browsing.

## Web client
Web client is located in `WebClient` directory. Build instructions:
```PowerShell
cd WebClient
npm install
npm run build
```

Then you have to copy all the files from `WebClient/dist` and `WebClient/static` directories into `static/` directory where the server is located. These files will be hosted by a WebsiteCacher to a client with offline webpages.
```PowerShell
cp WebClient/dist/*
```


## Database
To initialize database please run the following commands in Package Manager Console (PMC)

```PowerShell
Add-Migration WebsiteCacherDatabase
Update-Database
```
