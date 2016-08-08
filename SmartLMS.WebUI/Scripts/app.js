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

    $public.initializeSlimControl = function (restart) {
        if ($private.resize || restart) {
            $('.sidebar-container').slimScroll({ destroy: true });
        }
        $('.sidebar-container').slimScroll({
            height: window.innerHeight - $(".navbar").height() - $(".footer").height() - 40,
            railOpacity: 0.4,
            wheelStep: 10,
            size: 10
        });
        $private.resize = true;
    };

    $private.resizeSlimControl = function () {
        if ($private.mhResizeTimeout) {
            window.clearTimeout($private.mhResizeTimeout);
        }
        $private.mhResizeTimeout = 0;
        $private.mhResizeTimeout = window.setTimeout(doResizeStuff, 100);
    };

    function doResizeStuff() {
        $private.mhResizeTimeout = 0;
        $public.initializeSlimControl(false);
    }

    $(function () {
        $.each($("input[data-autocomplete]"), $private.createAutoComplete);
        $private.initializeDropdownJs();
        $private.initializeToastr();
        $public.initializeCarouselMulti();
        $public.initializeSlimControl(false);
        $(window).on("resize", $private.resizeSlimControl);
        
        $(document).ajaxError(function (event, xhr, options, thrownError) {
            var erro = $(xhr.responseText).filter("span").find("h2 > i").text();
            if (erro != "")
                $public.toastr["error"](erro, "Erro não tratado")
        });
    });


    return $public;

}());

