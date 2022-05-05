const intervalDelay = 60000;
const urlApi = "http://localhost:5297/api/Temperature";
//const urlApi = "http://jeanrasin-001-site1.itempurl.com/api/Temperature";

const getData = function () {
    fetch(urlApi)
        .then((response) => {
            return response.json();
        })
        .then((data) => {
            console.log(data);

            let tbodyStr = "";

            data.forEach(function (item, index, array) {

                let dateStr = item.date.replace("T", " ");
                tbodyStr += `
                        <tr>
                            <th>${item.id}</th>
                            <th>${dateStr}</th>
                            <th>${item.temperatureData}</th>
                            <th>${item.humidity}</th>
                        </tr>`;
            });

            let tbodyNode = document.querySelector('#temperatureTable tbody')
            tbodyNode.innerHTML = tbodyStr
        });
}

const getDataInterval = function () {
    getData();

    setInterval(function () {
        getData();
    }, intervalDelay);
}
