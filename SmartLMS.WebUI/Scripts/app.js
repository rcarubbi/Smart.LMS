
Handlebars.registerHelper('times', function (n, block) {
    var accum = '';
    for (var i = 0; i < n; ++i)
        accum += block.fn(i+1);
    return accum;
});

Handlebars.registerHelper("greaterThan", function (value1, value2, options) {
    if (value1 > value2)
        return options.fn(this);
});

Handlebars.registerHelper("lessThanOrEquals", function (value1, value2, options) {
    if (value1 <= value2)
        return options.fn(this);
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
        $public.initializeCarouselMulti();
    });

    return $public;

}());