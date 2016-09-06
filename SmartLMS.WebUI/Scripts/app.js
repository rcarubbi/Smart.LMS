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

Handlebars.registerHelper("toShortDateString", function(datetime) {
    return moment(datetime).format("DD/MM/YYYY");
})


SmartLMS.isMobile = /Android|webOS|iPhone|iPad|iPod|BlackBerry|IEMobile|Opera Mini/i.test(navigator.userAgent);
 



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

    $private.initializeMultiselect = function () {
        $(".multiselect").each(function (index, item) {

            $(item).multiselect({
                enableFiltering: $(item).data("multiselect-filter") == "True",
                enableClickableOptGroups: $(item).data("multiselect-group") == "True",
                enableCollapsibleOptGroups: $(item).data("multiselect-group") == "True",
                enableCaseInsensitiveFiltering:  $(item).data("multiselect-filter") == "True",
                filterPlaceholder: 'Pesquisar',
                nonSelectedText: 'Nenhum item selecionado',
                nSelectedText: 'selecionado(s)',
                allSelectedText: 'Todos',
                buttonClass: 'btn btn-default',
                maxHeight: '400',
                templates: {
                    filterClearBtn: '<span class="input-group-btn"><button class="btn btn-default btn-sm btn-simple multiselect-clear-filter" type="button"><i class="glyphicon glyphicon-remove-circle"></i></button></span>',
                }
            });
        });
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

        $(".carousel-inner").swipe({
            //Generic swipe handler for all directions
            swipeLeft: function (event, direction, distance, duration, fingerCount) {
                $(this).parent().carousel('next');
            },
            swipeRight: function () {
                $(this).parent().carousel('prev');
            },                
            //Default is 75px, set to 0 for demo so any distance triggers swipe
            threshold: 0
        });

        $(".carousel-inner a").swipe({
            tap: function () {
                document.location.href = $(this).attr("href");
            }
        });
    };

    $public.initializeSlimControl = function (restart) {
        if (!SmartLMS.isMobile) {
            if ($private.resize || restart) {
                $('.sidebar-container').slimScroll({ destroy: true });
            }
            $('.sidebar-container').slimScroll({
                height: window.innerHeight - $(".navbar").height() - $(".footer").height() - 40,
                railOpacity: 0.4,
                size: 10,
            });
            $private.resize = true;
        }
        else
        {
            $('.sidebar-container').css({
                "height": (window.innerHeight - $(".navbar").height() - $(".footer").height() - 40) + "px",
                "overflow" : "auto"
            });
        }
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
        moment.locale('pt-br');

        $.each($("input[data-autocomplete]"), $private.createAutoComplete);
        $private.initializeDropdownJs();
        $private.initializeToastr();
        $public.initializeCarouselMulti();
        $public.initializeSlimControl(false);
        $private.initializeMultiselect();
        $(window).on("resize", $private.resizeSlimControl);
        
        $(document).ajaxError(function (event, xhr, options, thrownError) {
            var erro = $(xhr.responseText).filter("span").find("h2 > i").text();
            if (erro != "")
                $public.toastr["error"](erro, "Erro não tratado")
        });
    });


    return $public;

}());

