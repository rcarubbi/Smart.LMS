SmartLMS.Vimeo = (function (Vimeo) {
    var $private = {}, $public = {};

    $public.initialize = function (classId, updateProgressUrl, watchedSeconds) {
        $private.progress = [];
        $private.classId = classId;
        $private.updateProgressUrl = updateProgressUrl;
        $private.pendingSeektoTime = parseFloat(watchedSeconds);

        var player = new Vimeo.Player("videoFrame");

        player.on("play", function () {
            if ($private.pendingSeektoTime > 0) {
                player.seekTo($private.pendingSeektoTime, true);
                $private.pendingSeektoTime = 0;
            }

            setInterval($private.updateProgress, 5000);
        });

        player.on("ended", $private.vimeoEndedEvent);
        player.on("timeupdate", $private.vimeoPlayProgressEvent);
        player.on("pause", $private.vimeoPauseEvent);
        player.ready();
    }



    $private.vimeoPauseEvent = function (data) {
        $private.updateProgress();
    }

    $private.vimeoEndedEvent = function (data) {
        $private.updateProgress();
    }

    $private.updateProgress = function () {
        if ($private.lastSent === $private.lastUpdate) {
            return;
        }

        $private.lastSent = $private.lastUpdate;

        var progressInfo = $private.progress[$private.progress.length - 1];

        $private.progress = [];

        $.ajax({
            type: "POST",
            contentType: "application/json; charset=utf-8",
            url: $private.updateProgressUrl,
            data: JSON.stringify({
                Percentual: Math.ceil(progressInfo.percent * 100),
                ClassId: $private.classId,
                WatchedSeconds: progressInfo.seconds
            }),

        });

        $private.updateProgressBar(progressInfo.seconds, progressInfo.percent);

    };

    $private.updateProgressBar = function (watchedSeconds, percentual) {
        $private.pendingSeektoTime = watchedSeconds;
        $(".fa.fa-laptop.active").parent().parent().find(".progress-bar")
            .css("width", (percentual * 100) + "%");
        $(".fa.fa-laptop.active").parent().parent().find(".sr-only").text((percentual * 100) + "% done");
    }

    $private.vimeoPlayProgressEvent = function (data) {
        var currentPercent = Math.ceil(data.percent * 100);
        if ($private.progress.indexOf(currentPercent) !== -1) {
            return;
        }

        var timestamp = (new Date()).getTime();
        timestamp = Math.floor(timestamp / 1000);

        $private.progress.push(data);
        $private.lastUpdate = timestamp;
    };

    return $public;
});
