
var dataTable;
$(document).ready(function () {
    loadDataTable();
});

function loadDataTable() {
    dataTable = $('#tblData').DataTable({
        columnDefs: [{
            "defaultContent": "-",
            "targets": "_all"
        }],
        "ajax": {
            url: '/admin/company/getall'
        },
        "columns": [
            { data: 'name', "width": "auto%" },
            { data: 'streetAddress', "width": "auto" },
            { data: 'city', "width": "auto" },
            { data: 'state', "width": "auto" },
            { data: 'postalCode', "width": "auto" },
            { data: 'phoneNumber', "width": "auto" },
            {
                data: null,
                "render": function (data, type, row) {
                    return `
                        <div class="w-75 btn-group" role="group">
                            <a href="/admin/company/upsert?id=${data.id}" class="btn btn-primary mx-2">
                                <i class="bi bi-pencil-square"></i> Edit
                            </a> 
                             <a  onClick=Delete('/admin/company/delete/${data.id}') class="btn btn-outline-danger mx-2">
                               <i class="bi bi-trash-fill"></i> Delete
                            </a>
                        </div>`;
                },
                "width": "auto"
            },
        ]
    });
}


function Delete(url) {
    Swal.fire({
        title: "Are you sure?",
        text: "You won't be able to revert this!",
        icon: "warning",
        showCancelButton: true,
        confirmButtonColor: "#3085d6",
        cancelButtonColor: "#d33",
        confirmButtonText: "Yes, delete it!"
    }).then((result) => {
        if (result.isConfirmed) {
            $.ajax({
                url: url,
                type: 'DELETE',
                success: function (data) {
                    dataTable.ajax.reload();
                    toastr.success(data.message);
                }
            })
        }
    });
}

