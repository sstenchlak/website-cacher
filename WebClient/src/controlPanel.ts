document.addEventListener("DOMContentLoaded", () => {
    document.getElementById("pageQueriesRefresh").addEventListener("click", async () => {
        let table = document.getElementById("pageQueries");
        table.innerHTML = "<tr><td colspan='6'>LOADING</td></tr>";
        let result = await fetch("/website-cacher://page-queries/list");
        let data = await result.json();
        table.innerHTML = "";
        for (let item of data) {
            table.innerHTML += `<tr><td>#${item.id}</td><td><a href='/${item.url}'>${item.url}</a></td><td>${item.depth}</td><td>${item.time} s</td><td><pre>${item.page_regexp}</pre></td><td><pre>${item.media_regexp}</pre></td></tr>`;
        }
    });
});