SmartLMS.Pagination = (function () {
    var $private = {}, $public = {};

    $public.init = function (config) {
        $public.Page = 1;
        $private.config = config || {};
        $private.config.paginationContainerId = config.paginationContainerId || "pagination-container";
        $private.config.paginationTemplateId = config.paginationTemplateId || "pagination-template";
        $private.config.resultContainerId = config.resultContainerId || "result-container";
        $private.config.templateResultId = config.templateResultId || "result-template";
        $private.config.onPageChanged = config.onPageChanged || function (page) { };
        $("#" + $private.config.paginationContainerId).on("click", ".pagination-page", $private.goToPage);
        $("#" + $private.config.paginationContainerId).on("click", ".pagination-next", $private.goToNextPage);
        $("#" + $private.config.paginationContainerId).on("click", ".pagination-previous", $private.goToPreviousPage);
        $private.PaginationTemplate = Handlebars.compile($("#" + $private.config.paginationTemplateId).html());
        $private.ResultTemplate = Handlebars.compile($("#" + $private.config.templateResultId).html());
    };

    $private.goToPage = function () {
        $public.Page = $(this).text();
        $private.config.onPageChanged($public.Page).done($public.refreshResults);
    };

    $private.goToNextPage = function () {
        $public.Page++;
        $private.config.onPageChanged($public.Page).done($public.refreshResults);
    };

    $private.goToPreviousPage = function () {
        $public.Page--;
        $private.config.onPageChanged($public.Page).done($public.refreshResults);
    };

    $private.getMinPage = function (pageCount, pageIndex) {
        pageIndex = parseInt(pageIndex);
        if (pageIndex < 4) {
            return 1;
        }
        else if (pageIndex > pageCount - 3) {
            return pageCount - 4;
        }
        else {
            return pageIndex - 2;
        }
    };

    $private.getMaxPage = function (pageCount, pageIndex) {
        pageIndex = parseInt(pageIndex);
        if (pageIndex < 4) {
            if (pageCount < 6) {
                return pageCount + 1;
            }
            else {
                return 6;
            }
        }
        else if (pageIndex > pageCount - 3) {
            return pageCount + 1;
        }
        else {
            return pageIndex + 3;
        }
    };

    $public.refreshResults = function (data) {
        $("#" + $private.config.resultContainerId).html($private.ResultTemplate(data));
        if (data.HasNext) {
            $(".pagination-next").show();
        }
        else {
            $(".pagination-next").hide();
        }

        if (data.HasPrevious) {
            $(".pagination-previous").show();
        }
        else {
            $(".pagination-previous").hide();
        }

        $("#" + $private.config.paginationContainerId).html($private.PaginationTemplate({
            pageCount: Math.ceil(data.Count / 8),
            hasPrevious: data.HasPrevious,
            hasNext: data.HasNext,
            minPage: $private.getMinPage(Math.ceil(data.Count / 8), $public.Page),
            maxPage: $private.getMaxPage(Math.ceil(data.Count / 8), $public.Page),
            currentPage: $public.Page
        }));
    };

    return $public;
}());