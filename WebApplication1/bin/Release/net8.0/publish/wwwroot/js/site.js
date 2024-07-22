var loc = window.location.pathname;

if (loc == '/Trip/Create' || loc == '/') {
    $(document).ready(function () {        
        setUpAutocomplete();
        setUpDate();
    })
}

function setUpDate() {
    var today = new Date();    

    var yyyy = today.getFullYear();
    var mm = String(today.getMonth() + 1).padStart(2, '0'); 
    var dd = String(today.getDate()).padStart(2, '0');
    var formattedDate = yyyy + '-' + mm + '-' + dd;   

    var dateInput = $(".dateinput");
    var inputValue = dateInput.attr("value");

    if (loc == '/') {       

        if (inputValue == "") {
            dateInput.attr({"value": formattedDate, });
        }

        dateInput.attr({"min": formattedDate,});
    }

    else {
        
        var hh = String(today.getHours() + 1).padStart(2, '0');      

        if (parseInt(hh) == 24) { hh = '00'; formattedDate = yyyy + '-' + mm + '-' + (parseInt(dd) + 1).toString(); };

        var minm = String(today.getMinutes()+2).padStart(2, '0');
        var formattedTime = "T" + hh + ":" + minm

        if (inputValue == "") {

            dateInput.attr({"value": formattedDate + formattedTime,});
        }

        dateInput.attr({"min": formattedDate + formattedTime,});
    }
}
function setUpAutocomplete() {

    $(".Autocomplete").autocomplete({
        source: function (request, response) {
            $.ajax({
                url: "/Trip/LoadCityAutoComplete",
                type: "GET",
                dataType: "json",
                data: { Prefix: request.term },
                success: function (data) {
                    response(data, function (item) {
                        return { item };
                    })
                }
            })
        }

    });
}