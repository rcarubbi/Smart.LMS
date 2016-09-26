SmartLMS.Paginacao = (function () {
    var $private = {}, $public = {};

    $public.init = function (config) {
        $public.Pagina = 1;
        $private.config = config || {};
        $private.config.paginacaoContainerId = config.paginacaoContainerId || "paginacao-container";
        $private.config.paginacaoTemplateId = config.paginacaoTemplateId || "paginacao-template";
        $private.config.resultadoContainerId = config.resultadoContainerId || "resultado-container";
        $private.config.templateResultadoId = config.templateResultadoId || "resultado-template";
        $private.config.onPageChanged = config.onPageChanged || function (pagina) { };
        $("#" + $private.config.paginacaoContainerId).on("click", ".paginacao-pagina", $private.IrParaPagina);
        $("#" + $private.config.paginacaoContainerId).on("click", ".paginacao-proxima", $private.ProximaPagina);
        $("#" + $private.config.paginacaoContainerId).on("click", ".paginacao-anterior", $private.PaginaAnterior);
        $private.PaginacaoTemplate = Handlebars.compile($("#" + $private.config.paginacaoTemplateId).html());
        $private.ResultadoTemplate = Handlebars.compile($("#" + $private.config.templateResultadoId).html());
    };

    $private.IrParaPagina = function () {
        $public.Pagina = $(this).text();
        $private.config.onPageChanged($public.Pagina).done($public.AtualizarResultados);
    };

    $private.ProximaPagina = function () {
        $public.Pagina++;
        var termo = $("#termo").val();
        $private.config.onPageChanged($public.Pagina).done($public.AtualizarResultados);
    };

    $private.PaginaAnterior = function () {
        $public.Pagina--;
        $private.config.onPageChanged($public.Pagina).done($public.AtualizarResultados);
    };

    $private.CalcularPaginaMinima = function (quantidade, pagina) {
        pagina = parseInt(pagina);
        if (pagina < 4) {
            return 1;
        }
        else if (pagina > quantidade - 3) {
            return quantidade - 4;
        }
        else {
            return pagina - 2;
        }
    };

    $private.CalcularPaginaMaxima = function (quantidade, pagina) {
        pagina = parseInt(pagina);
        if (pagina < 4) {
            if (quantidade < 6) {
                return quantidade + 1;
            }
            else {
                return 6;
            }
        }
        else if (pagina > quantidade - 3) {
            return quantidade + 1;
        }
        else {
            return pagina + 3;
        }
    };

    $public.AtualizarResultados = function (data) {
        $("#" + $private.config.resultadoContainerId).html($private.ResultadoTemplate(data));
        if (data.HasNext) {
            $(".paginacao-proxima").show();
        }
        else {
            $(".paginacao-proxima").hide();
        }

        if (data.HasPrevious) {
            $(".paginacao-anterior").show();
        }
        else {
            $(".paginacao-anterior").hide();
        }

        $("#" + $private.config.paginacaoContainerId).html($private.PaginacaoTemplate({
            paginas: Math.ceil(data.Count / 8),
            possuiAnterior: data.HasPrevious,
            possuiProxima: data.HasNext,
            paginaMin: $private.CalcularPaginaMinima(Math.ceil(data.Count / 8), $public.Pagina),
            paginaMax: $private.CalcularPaginaMaxima(Math.ceil(data.Count / 8), $public.Pagina),
            paginaCorrente: $public.Pagina
        }));

   
      
     
    };

    return $public;

}());