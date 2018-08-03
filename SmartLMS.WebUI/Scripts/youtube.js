SmartLMS.Youtube = (function (Youtube) {
    var $private = {}, $public = {};

    $public.initialize = function (classId, updateProgressUrl, watchedSeconds) {
      
        $private.progress = [];
        $private.pendingSeektoTime = parseFloat(watchedSeconds);
        $private.classId = classId;
        $private.updateProgressUrl = updateProgressUrl;

        $private.player = new Youtube.Player('videoFrame',
            {
                events: {
                    'onStateChange': $private.onPlayerStateChange
                }
            });
    }

    $private.onPlayerStateChange = function (event) {
        switch (event.data) {
            case Youtube.PlayerState.PLAYING:
                if ($private.pendingSeektoTime > 0) {
                    $private.player.seekTo($private.pendingSeektoTime, true);
                    $private.pendingSeektoTime = 0;
                }
                if ($private.monitor === undefined) $private.monitor = setInterval($private.updateProgress, 5000);
                break;
            case Youtube.PlayerState.ENDED:
            case Youtube.PlayerState.PAUSED:
                $private.updateProgress();
                clearInterval($private.monitor);
                $private.monitor = undefined;
                break;
             
        }
    }
 
    $private.updateProgress = function () {
        $private.getPlayProgress();
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
            })
        });

        $private.updateProgressBar(progressInfo.seconds, progressInfo.percent);

    };

    $private.updateProgressBar = function (watchedSeconds, percentual) {
        $private.pendingSeektoTime = watchedSeconds;
        $(".fa.fa-laptop.active").parent().parent().find(".progress-bar")
            .css("width", (percentual * 100) + "%");
        $(".fa.fa-laptop.active").parent().parent().find(".sr-only").text((percentual * 100) + "% done");
    }

    $private.getPlayProgress = function () {
        var data = {
            percent: $private.player.getCurrentTime() / $private.player.getDuration(),
            seconds: $private.player.getCurrentTime(),
            duration: $private.player.getDuration()
        };

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
