SmartLMS.ClassroomProxy = (function() {
    $public = {}, $private = {};


    $public.search = function(term, searchFieldname, page) {
        return $.ajax({
            type: "POST",
            url: SmartLMS.api + "Classroom/Search",
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            data: JSON.stringify({ term, searchFieldname, page })
        });
    };

    $public.listStudents = function(id) {
        return $.ajax({
            type: "POST",
            url: SmartLMS.api + "Classroom/ListStudents",
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            data: JSON.stringify({ id })
        });
    };

    $public.listCourses = function(id) {
        return $.ajax({
            type: "POST",
            url: SmartLMS.api + "Classroom/ListCourses",
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            data: JSON.stringify({ id })
        });
    };

    $public.delete = function(id) {
        return $.ajax({
            type: "POST",
            url: SmartLMS.api + "Classroom/Delete",
            contentType: "application/json; charset=utf-8",
            data: JSON.stringify({ id })
        });
    };

    return $public;
}());