window.AIV = {
    init: function () {
        $(document).ready(function () {

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
        });
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