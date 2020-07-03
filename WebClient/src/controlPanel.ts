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

    let form = <HTMLFormElement>document.getElementById("newPageQueryForm");

    form.onsubmit = function (e) {
        // stop the regular form submission
        e.preventDefault();

        // collect the form data while iterating over the inputs
        var data: { [key: string]: string } = {};
        for (var i = 0, ii = form.length; i < ii; ++i) {
          var input: any = form[i];
          if (input.name) {
            data[input.name] = input.value;
          }
        }

        // construct an HTTP request
        var xhr = new XMLHttpRequest();
        xhr.open(form.method, form.action, true);
        xhr.setRequestHeader('Content-Type', 'application/json; charset=UTF-8');

        // send the collected data as JSON
        xhr.send(JSON.stringify(data));

        xhr.onloadend = function () {
            alert("The request has ID #" + JSON.parse(xhr.responseText).id + ".");
        };
      };
});