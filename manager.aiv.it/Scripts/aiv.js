﻿window.AIV = {
    init: function () {
        $(document).ready(function () {
            document.createElementSVG = function (tag) {
                var svgtag = document.createElementNS("http://www.w3.org/2000/svg", tag);
                if (!svgtag.append)
                    svgtag.append = svgtag.appendChild;
                return svgtag;
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

            /* ACTIVATION on checkbox inputs */

            var activator = function () {
                var group = $(this).parents(".activable-group").find(".active").each(function () {
                    $(this).removeClass("active");
                });

                $(this).parent().toggleClass("active");
            };

            $(".activable").click(activator);


            /* IMAGE tags as checkboxes */
            var imgCh = $(".image-checkbox ~ label");
            imgCh.css('background-image', 'url(' + imgCh.attr("image") + ')');
            $(".image-checkbox").change(function () {
                console.log($(this).is(":checked"));
            });

            var openLoadingModal = function () {
                var loadingOverlay = $("#loading-overlay");
                if (!loadingOverlay.hasClass("open")) {
                    window.AIV.openLoading();
                }
            };

            // window.AIV.closeLoading();
            // FOR TESTING: 
            // ------------
            //$("*[href]").click(window.AIV.openLoading);
            //$(window).on('hashchange', window.AIV.openLoading);
            window.onbeforeunload = openLoadingModal;

            // INTERCEPT ALL AJAX REQUESTS
            var originalXHROpening = XMLHttpRequest.prototype.open;
            XMLHttpRequest.prototype.open = function () {
                openLoadingModal();
                originalXHROpening.apply(this, arguments);
            };

            // close loading on landing after 500 ms
            setTimeout(function () {
                window.AIV.closeLoading();
            }, 1000);

            if (document.getElementById("starfield") != null) {
                window.AIV.setupStarfield("starfield");
            }
        });
    },
    setupStarfield : function(elementId){
        function $i(id) { return document.getElementById(id); }
        function $r(parent, child) { (document.getElementById(parent)).removeChild(document.getElementById(child)); }
        function $t(name) { return document.getElementsByTagName(name); }
        function $c(code) { return String.fromCharCode(code); }
        function $h(value) { return ('0' + Math.max(0, Math.min(255, Math.round(value))).toString(16)).slice(-2); }
        function _i(id, value) { $t('div')[id].innerHTML += value; }
        function _h(value) { return !hires ? value : Math.round(value / 2); }

        function get_screen_size() {
            return Array($(window).width(), $(window).height());
        }

        var url = document.location.href;

        var flag = true;
        var test = true;
        var n = parseInt((url.indexOf('n=') != -1) ? url.substring(url.indexOf('n=') + 2, ((url.substring(url.indexOf('n=') + 2, url.length)).indexOf('&') != -1) ? url.indexOf('n=') + 2 + (url.substring(url.indexOf('n=') + 2, url.length)).indexOf('&') : url.length) : 512);
        var w = 0;
        var h = 0;
        var x = 0;
        var y = 0;
        var z = 0;
        var star_color_ratio = 0;
        var star_x_save, star_y_save;
        var star_ratio = 256;
        var star_speed = 1;
        var star_speed_save = 0;
        var star = new Array(n);
        var color;
        var opacity = 0.1;

        var cursor_x = 0;
        var cursor_y = 0;
        var mouse_x = 0;
        var mouse_y = 0;

        var canvas_x = 0;
        var canvas_y = 0;
        var canvas_w = 0;
        var canvas_h = 0;
        var context;

        var key;
        var ctrl;

        var timeout;
        var fps = 0;

        function init() {
            var a = 0;
            for (var i = 0; i < n; i++) {
                star[i] = new Array(5);
                star[i][0] = Math.random() * w * 2 - x * 2;
                star[i][1] = Math.random() * h * 2 - y * 2;
                star[i][2] = Math.round(Math.random() * z);
                star[i][3] = 0;
                star[i][4] = 0;
            }
            var starfield = $i(elementId);
            starfield.style.position = 'absolute';
            starfield.width = w;
            starfield.height = h;
            context = starfield.getContext('2d');
            //context.lineCap='round';

            var bgCanvas = document.createElement("canvas");
            var bgCanvasCtx = bgCanvas.getContext("2d");
            bgCanvas.width = $(window).width();
            bgCanvas.height = $(window).height();

            var bgImage = new Image();
            bgImage.onload = function () {
                var imageW = bgImage.width;
                var imageH = bgImage.height;

                var ratio;
                if(imageW < imageH){
                    ratio = imageW / bgCanvas.width;
                } else {
                    ratio = imageH / bgCanvas.height;
                }

                var expectedW = imageW / ratio;
                var expectedH = imageH / ratio;

                var fixRatioW = 1;
                if (bgCanvas.width - expectedW > 0) {
                    fixRatioW = bgCanvas.width / expectedW;
                }

                var fixRatioH = 1;
                if (bgCanvas.height - expectedH > 0) {
                    fixRatioH = bgCanvas.height / expectedH;
                }

                var finalW = (expectedW) * fixRatioW;
                var finalH = (expectedH) * fixRatioH;

                bgCanvasCtx.drawImage(bgImage, 0, 0, bgImage.width, bgImage.height, 0, 0, finalW, finalH);
                context.fillStyle = context.createPattern(bgCanvas, 'no-repeat');
            };
            bgImage.src = "/Content/galaxy3.jpg";

            context.strokeStyle = 'rgb(255,255,255)';
        }

        function anim() {
            mouse_x = cursor_x - x;
            mouse_y = cursor_y - y;
            context.fillRect(0, 0, w, h);
            for (var i = 0; i < n; i++) {
                test = true;
                star_x_save = star[i][3];
                star_y_save = star[i][4];
                star[i][0] += mouse_x >> 8; if (star[i][0] > x << 1) { star[i][0] -= w << 1; test = false; } if (star[i][0] < -x << 1) { star[i][0] += w << 1; test = false; }
                star[i][1] += mouse_y >> 8; if (star[i][1] > y << 1) { star[i][1] -= h << 1; test = false; } if (star[i][1] < -y << 1) { star[i][1] += h << 1; test = false; }
                star[i][2] -= star_speed; if (star[i][2] > z) { star[i][2] -= z; test = false; } if (star[i][2] < 0) { star[i][2] += z; test = false; }
                star[i][3] = x + (star[i][0] / star[i][2]) * star_ratio;
                star[i][4] = y + (star[i][1] / star[i][2]) * star_ratio;
                if (star_x_save > 0 && star_x_save < w && star_y_save > 0 && star_y_save < h && test) {
                    context.lineWidth = (1 - star_color_ratio * star[i][2]) * 2;
                    context.beginPath();
                    context.moveTo(star_x_save, star_y_save);
                    context.lineTo(star[i][3], star[i][4]);
                    context.stroke();
                    context.closePath();
                }
            }
            timeout = setTimeout(anim, (fps) > 17 ? fps : 17);

            // TODO: 
            // trovare modo di ingrandire tela
            //context.translate(w / 2, h / 2);
            //context.rotate(2 * Math.PI / 18000);
            //context.translate(-w / 2, -h / 2);
        }

        function move(evt) {
            evt = evt || event;
            cursor_x = evt.pageX - canvas_x;
            cursor_y = evt.pageY - canvas_y;
        }

        function key_manager(evt) {
            evt = evt || event;
            key = evt.which || evt.keyCode;
            //ctrl=evt.ctrlKey;
            switch (key) {
                case 27:
                    flag = flag ? false : true;
                    if (flag) {
                        timeout = setTimeout(anim, (fps) > 17 ? fps : 17);
                    }
                    else {
                        clearTimeout(timeout);
                    }
                    break;
            }
            top.status = 'key=' + ((key < 100) ? '0' : '') + ((key < 10) ? '0' : '') + key;
        }

        function mouse_wheel(evt) {
            evt = evt || event;
            var delta = 0;
            if (evt.wheelDelta) {
                delta = evt.wheelDelta / 120;
            }
            else if (evt.detail) {
                delta = -evt.detail / 3;
            }
            star_speed += (delta >= 0) ? -0.2 : 0.2;
            if (evt.preventDefault) evt.preventDefault();
        }

        function start() {
            resize();
            anim();

            setTimeout(function () {
                resize();
                anim();
            }, 300);
        }

        function resize() {
            w = get_screen_size()[0];
            h = get_screen_size()[1];
            x = Math.round(w / 2);
            y = Math.round(h / 2);
            z = (w + h) / 2;
            star_color_ratio = 1 / z;
            cursor_x = x;
            cursor_y = y;
            init();
        }

        document.onmousemove = move;

        $(document).ready(start);
        $(window).resize(start);
        start();
    },
    openLoading: function () {
        //window.AIV.initGameboy();
        //$("#loading-overlay").addClass('open');
        //$(".gameboy .screen").focus();
    },
    closeLoading: function () {
        //$("#loading-overlay").removeClass('open');
        //$(".gameboy .screen").blur();
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
                            .attr("y", 0)
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