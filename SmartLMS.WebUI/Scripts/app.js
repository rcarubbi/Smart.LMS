Handlebars.registerHelper("math", function (lvalue, operator, rvalue, options) {
    lvalue = parseFloat(lvalue);
    rvalue = parseFloat(rvalue);

    return {
        "+": lvalue + rvalue,
        "-": lvalue - rvalue,
        "*": lvalue * rvalue,
        "/": lvalue / rvalue,
        "%": lvalue % rvalue
    }[operator];
});

Handlebars.registerHelper('times', function (n, block) {
    var accum = '';
    for (var i = 0; i < n; ++i)
        accum += block.fn(i + 1);
    return accum;
});

Handlebars.registerHelper("greaterThan", function (value1, value2, options) {
    if (value1 > value2)
        return options.fn(this);

    return options.inverse(this);
});

Handlebars.registerHelper("lessThanOrEquals", function (value1, value2, options) {
    if (value1 <= value2)
        return options.fn(this);
    return options.inverse(this);
});

Handlebars.registerHelper("equals", function (value1, value2, options) {
    if (value1 == value2)
        return options.fn(this);

    return options.inverse(this);
});

Handlebars.registerHelper('for', function (from, to, incr, block) {
    var accum = '';
    for (var i = from; i < to; i += incr)
        accum += block.fn(i);
    return accum;
});

var SmartLMS = {};
SmartLMS.api = location.protocol + '//' + location.hostname + (location.port ? ':' + location.port : '') + "/";
 


SmartLMS.App = (function () {
    var $public = {}, $private = {};
    $private.createAutoComplete = function () {
        var $input = $(this);
        var options = {
            source: $input.attr("data-autocomplete")
        };

        $input.autocomplete(options);
    }

    $private.initializeToastr = function () {
        $public.toastr = toastr;
        $public.toastr.options = {
            "closeButton": true,
            "debug": false,
            "progressBar": true,
            "positionClass": "toast-bottom-right",
            "onclick": null,
            "showDuration": "400",
            "hideDuration": "1000",
            "timeOut": "3000",
            "extendedTimeOut": "1000",
            "showEasing": "swing",
            "hideEasing": "linear",
            "showMethod": "fadeIn",
            "hideMethod": "fadeOut"
        }
    };

    $private.initializeDropdownJs = function () {
        $("select.dropdownjs").dropdownjs();
    };

    $public.initializeCarouselMulti = function () {
        $('.carousel[data-type="multi"] .item').each(function () {
            var next = jQuery(this).next();
            if (!next.length) {
                next = jQuery(this).siblings(':first');
            }
            next.children(':first-child').clone().appendTo(jQuery(this));

            for (var i = 0; i < 2; i++) {
                next = next.next();
                if (!next.length) {
                    next = jQuery(this).siblings(':first');
                }
                next.children(':first-child').clone().appendTo($(this));
            }
        });
    };

    $(function () {
        $.each($("input[data-autocomplete]"), $private.createAutoComplete);
        $private.initializeDropdownJs();
        $private.initializeToastr();
        $public.initializeCarouselMulti();
        $(document).ajaxError(function (event, xhr, options, thrownError) {
            var erro = $(xhr.responseText).filter("span").find("h2 > i").text();
            if (erro != "")
                $public.toastr["error"](erro, "Erro não tratado")
        });
    });


    return $public;

}());

