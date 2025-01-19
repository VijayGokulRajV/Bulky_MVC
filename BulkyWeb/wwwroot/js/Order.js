
var dataTable;
$(document).ready(function () {
    var url = window.location.search;
    if (url.includes("pending")) {
        loadDataTable("pending");
    }
    else{
        if (url.includes("inprocess")) {
            loadDataTable("inprocess");
        }
        else {
            if (url.includes("completed")) {
                loadDataTable("completed");
            }
            else {
                if (url.includes("approved")) {
                    loadDataTable("approved");
                }
                else {
                    loadDataTable("all");
                }
            }
        }
    }
});

function loadDataTable(status) {
    dataTable = $('#tblData').DataTable({
        columnDefs: [{
            "defaultContent": "-",
            "targets": "_all"
        }],
        "ajax": {
            url: '/admin/order/getall?status=' + status
        },
        "columns": [
            { data: 'id', "width": "10%" },
            { data: 'name', "width": "20%" },
            { data: 'phoneNumber', "width": "20%" },
            { data: 'applicationUser.email', "width": "20%" },
            { data: 'orderStatus', "width": "15%^" },
            { data: 'orderTotal', "width": "15%^" },
            {
                data: null,
                "render": function (data, type, row) {
                    return `
                        <div class="w-75 btn-group" role="group">
                            <a href="/admin/order/details?orderId=${data.id}" class="btn btn-primary mx-2">
                                <i class="bi bi-pencil-square"></i> 
                            </a> 
                  
                        </div>`;
                },
                "width": "10%"
            },
        ]
    });
}

