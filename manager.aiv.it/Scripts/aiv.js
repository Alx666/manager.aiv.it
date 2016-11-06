window.AIV = {
    init: function () {
        $(document).ready(function () {
            document.createElementSVG = function (tag) {
                return document.createElementNS("http://www.w3.org/2000/svg", tag);
            };

            window.AIV.openLoading();

            var switches = $("[switch]");
            if (switches && switches.bootstrapSwitch) {
                switches.bootstrapSwitch();
            }

            var pickers = $(".datepicker");
            if (pickers && pickers.datepicker) {
                pickers.datepicker();
            }

            var tableContainers = $(".table-container");
            var tables = tableContainers.find("table");
            function updateTableIndicators() {
                tables.each(function (table) {

                });
            }

            updateTableIndicators();

            var header = $(".navbar");
            $(document).bind('scroll', function () {
                var y = $(document).scrollTop();

                var headerHeight = header.height();
                var rowTitle = $(".row-title");
                if (headerHeight <= y) {
                    if (!rowTitle.hasClass("yscrolling")) {
                        $(".row-title").addClass("yscrolling");
                        $(".navbar-collapse").css({ 'position': 'fixed' });
                    }
                    if (!rowTitle.next().hasClass("yscrolling-next-sibling"))
                        rowTitle.next().addClass("yscrolling-next-sibling");

                } else {
                    rowTitle.removeClass("yscrolling");
                    $(".navbar-collapse").css({ 'position': 'relative' });
                    rowTitle.next().removeClass("yscrolling-next-sibling");
                }

                updateTableIndicators();
            });

            $(".image-to-zoom").click(function () {
                var name = $(this).attr("name");
                $(".image-to-unzoom-description h4").text(name);
                $(".zoom-panel").fadeTo(300, 1, function () { $(this).css({ pointerEvents: "all", display : "block" }); });
                $(".image-to-unzoom").attr("src", $(".image-to-zoom").attr("src"));
            });

            $(".image-to-unzoom").click(function () {
                $(".image-to-unzoom-description h4").text("");
                $(".zoom-panel").fadeTo(300, 0, function () { $(this).css({ pointerEvents: "none", display: "none" }); });
                $(".image-to-unzoom").attr("src", "");
            });


            var activator = function () {
                var group = $(this).parents(".activable-group").find(".active").each(function () {
                    $(this).removeClass("active");
                });

                $(this).parent().toggleClass("active");
            };

            $(".activable").click(activator);

            // window.AIV.closeLoading();
            // FOR TESTING: 
            // ------------
            //$("*[href]").click(window.AIV.openLoading);
            //$(window).on('hashchange', window.AIV.openLoading);
            window.onbeforeunload = window.AIV.openLoading;
            $(window).unload(window.AIV.openLoading);

            // INTERCEPT ALL AJAX REQUESTS
            var originalXHROpening = XMLHttpRequest.prototype.open;
            XMLHttpRequest.prototype.open = function () {
                var loadingOverlay = $("#loading-overlay");
                if (!loadingOverlay.hasClass("open")) {
                    loadingOverlay.addClass("open");
                }
                originalXHROpening.apply(this, arguments);
            };

            // close loading on landing after 500 ms
            setTimeout(function () {
                window.AIV.closeLoading();
            }, 1000);
        });
    },
    openLoading: function () {
        window.AIV.initGameboy();
        $("#loading-overlay").addClass('open');
        $(".gameboy .screen").focus();
    },
    closeLoading: function () {
        $("#loading-overlay").removeClass('open');
        $(".gameboy .screen").blur();
    },
    initGameboy : function(){
        var svg = $("#gameboy-screen-canvas");
        svg.attr("xmlns", "http://www.w3.org/2000/svg");
        svg.attr("xmlns:xlink", "http://www.w3.org/1999/xlink");

        var width = parseInt(svg.attr("width"));
        var height = parseInt(svg.attr("height"));
        var main = document.createElementSVG("g");
        svg.append(main);
        var spawnX = 300; // valore x nascita oggetti off-screen
        var ground = 240; // valore y terreno in game
        var unit = 2;
        var pixelColor = "#222222";
        var gameFullWidth = 5000 * unit;

        var groundRect = document.createElementSVG("rect");
        $(groundRect).attr("class", "gameboy-static-object ground")
                     .attr("fill", pixelColor)
                     .attr("width", gameFullWidth)
                     .attr("height", unit * 40)
                     .attr("x", -unit)
                     .attr("y", ground);
        //main.append(groundRect);

        var gameboyUIRect = document.createElementSVG("rect");
        $(gameboyUIRect).attr("x", 156)
                        .attr("y", 10)
                        .attr("width", 120)
                        .attr("height", 20)
                        .attr("stroke", pixelColor)
                        .attr("fill", "none");
        main.append(gameboyUIRect);

        var gameboyUIWaitMessageRect = document.createElementSVG("rect");
        $(gameboyUIWaitMessageRect).attr("x", 20)
                                   .attr("y", 220)
                                   .attr("width", 240)
                                   .attr("height", 86)
                                   .attr("stroke", pixelColor)
                                   .attr("fill", "none");
        main.append(gameboyUIWaitMessageRect);

        var gameboyUI = document.createElementSVG("text");
        $(gameboyUI).attr("x", 160)
                    .attr("y", 24)
                    .css({
                        fontFamily    : "VT323",
                        fontSize      : "18px",
                        textTransform : 'uppercase',
                        color         : pixelColor
                    })
                    .text("POINTS");
        main.append(gameboyUI);

        var gameboyGameTitle = document.createElementSVG("text");
        $(gameboyGameTitle).attr("x", 10)
                    .attr("y", 24)
                    .css({
                        fontFamily: "VT323",
                        fontSize: "18px",
                        textTransform: 'uppercase',
                        color: pixelColor
                    })
                    .text("AIV MANAGER");
        main.append(gameboyGameTitle);

        var gameboyUIWaitMessage = document.createElementSVG("text");
        $(gameboyUIWaitMessage).attr("x", 72)
                               .attr("y", 268)
                               .css({
                                   fontFamily: "VT323",
                                   fontSize: "28px",
                                   textTransform: 'uppercase',
                                   color: pixelColor
                               })
                               .text("PLEASE WAIT");
        main.append(gameboyUIWaitMessage);

        var pointsUICounter = document.createElementSVG("text");
        $(pointsUICounter).attr("x", 240)
                          .attr("y", 24)
                          .css({
                              fontFamily: "VT323",
                              fontSize: "18px",
                              textTransform: 'uppercase',
                              color: pixelColor
                          })
                          .text("100");
        main.append(pointsUICounter);

        var pleaseWaitMessage = document.createElementSVG("image");
        $(pleaseWaitMessage).attr("x", 12)
                            .attr("y", 12)
                            .attr("width", width - 24)
                            .attr("height", height - 24)
                            .attr("href", "/Content/alienbig.png")
                            .attr("xlink:href", "/Content/alienbig.png");
        main.append(pleaseWaitMessage);

        var gameLoop = setInterval(function () {
            var staticObjects = $(".gameboy-static-object");
            staticObjects.each(function (index, staticObject) {
                var target = $(staticObject);
                var x = parseInt(target.attr("x"));
                var w = parseInt(target.attr("width"));
                if (!isNaN(x) && !isNaN(w)) {
                    if(x < - (unit + w) && !target.hasClass("ground")){
                        target.remove();
                    } else {
                        target.attr("x", x - 10);
                    }
                }
            });

            var groundX = parseInt($(groundRect).attr("x"));
            if (!isNaN(groundX) && (groundX + gameFullWidth < spawnX)) {
                $(groundRect).attr("x", -unit);
            }
        }, 100); // 60 fps = (1000 ms / 60)

        var loadingMessageBlinker = setInterval(function () {
            $("#loading-overlay .gameboy .loading-message").toggleClass("hidden");
        }, 500); // ogni mezzo secondo

    },
    arrayBufferToImageSrc: function (buffer, domElementQueryString) {
        var uintArray = new Uint8Array(buffer);
        var base64 = StringView.bytesToBase64(uintArray);
        $(domElementQueryString).attr("src", "data:image/png;base64," + base64);
    },
    updateImagePreview: function (file, imageTagID) {
        var reader = new FileReader();
        reader.onload = function () {
            window.AIV.arrayBufferToImageSrc(this.result, imageTagID);
        };
        reader.readAsArrayBuffer(file);
    },
    submitForm: function (formName) {
        if(formName && document[formName])
            document[formName].submit();
    },
    onFileInputChange: function (input, callback) {
        if (input.files && input.files[0]) {
            var file = input.files[0];
            callback(file);
        }
    },
    click : function (query) {
        $(query).click();
    },
    previewAndSubmitOnFileChange: function (input, formName, imagePreviewTagID) {
        window.AIV.onFileInputChange(input, function (file) {
            window.AIV.updateImagePreview(file, "#navbar-profile-preview");
            window.AIV.submitForm("navbar_form_picture");
        });
    }
};

window.AIV.init();