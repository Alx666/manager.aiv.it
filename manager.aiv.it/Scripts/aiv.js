window.AIV = {
    colors : {
        success: "#468847",
        danger : "#b94a48",
        info: "#3a87ad",
        lightInfo: "#c6dfec",
        warning: "#c09853",
        muted  : "#999999"
    },
    callbacks: [],
    isFunction : function(input){
        return !!(input && input.constructor && input.call && input.apply);
    },
    register : function(callback){
        if (window.AIV.isFunction(callback)) {
            window.AIV.callbacks.push(callback);
        }
    },
    init: function () {
        $(document).ready(function () {

            window.AIV.callbacks.forEach(function (c) {
                c();
            });

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
    },
    d3PieChart: function (query, data) {
        var margin = 16;
        var width = 200;
        var height = 200;
        var innerW = width - margin;
        var innerH = height - margin;
        var radius = Math.min(innerW, innerH) / 2;
        var donutWidth = Math.floor((75 * innerW) / 360);
        var legendRectSize = 18;
        var legendSpacing = 4;

        var svg = d3.select(query)
          .append('svg')
          .attr('width', width)
          .attr('height', height)
          .append('g')
          .attr('transform', 'translate(' + (width / 2) +
            ',' + (height / 2) + ')');

        var arc = d3.arc()
          .innerRadius(radius - donutWidth)
          .outerRadius(radius);

        var pie = d3.pie()
          .value(function(d) { return d.count; })
          .sort(null);

        var tooltip = d3.select(query)
          .append('div')                                               
          .attr('class', 'tooltip');                                   

        tooltip.append('div')                                          
          .attr('class', 'label');                                     

        tooltip.append('div')                                          
          .attr('class', 'count');                                     

        tooltip.append('div')                                          
          .attr('class', 'percent');                                   

        data.forEach(function (d) {
            d.count = +d.count;
        });

        var path = svg.selectAll('path')
          .data(pie(data))
          .enter()
          .append('path')
          .attr('d', arc)
          .attr('fill', function (d, i) {
              return d.data.color;
          });

        svg.selectAll('.description')
            .data(pie(data))
            .enter()
            .append("text")
            .attr("class", "description")
            .style("text-anchor", "middle")
            .style("font-size", "22px")
            .style("font-wweight", "bold")
            .style("fill", function (d) {
                return d.data.textColor || "silver";
            })
            .attr("transform", function (d) {
                d.innerRadius = radius - donutWidth;
                d.outerRadius = radius;
                var c = arc.centroid(d);
                return "translate(" + c[0] + "," + (c[1] + 5) + ")";
            })
            .text(function (d) {
                return (d.data.count > 0) ? d.data.count : "";
            });

        path.on('mouseover', function (d) {
            var total = d3.sum(data.map(function (d) {                
                return d.count;                                       
            }));                                                      
            var percent = Math.round(1000 * d.data.count / total) / 10;
            tooltip.select('.label').style('color', d.data.color).html(d.data.label);
            tooltip.select('.count').html(d.data.count + ' / ' + total);               
            tooltip.select('.percent').html(percent + '%');            
            tooltip.style('display', 'block');                         
        });                                                           

        path.on('mouseout', function () {                             
            tooltip.style('display', 'none');                         
        });                                                           

        path.on('mousemove', function (d) {
            var mouse = d3.mouse(this);
            console.log(d3.event);
            tooltip.style('top', (d3.event.clientY) + 'px')
                 .style('left', (d3.event.clientX) + 'px');
        });                

        var legend = svg.selectAll('.legend')
          .data(data)
          .enter()
          .append('g')
          .attr('class', 'legend')
          .attr('transform', function (d, i) {
              var height = legendRectSize + legendSpacing;
              var offset = height * data.length / 2;
              var horz = -2 * legendRectSize;
              var vert = i * height - offset;
              return 'translate(' + horz + ',' + vert + ')';
          });

        legend.append('rect')
          .attr('width', legendRectSize)
          .attr('height', legendRectSize)
          .style('fill', function (d) {
              return d.color;
          })
          .style('stroke', function (d) {
              return d.color;
          });

        legend.append('text')
          .attr('x', legendRectSize + legendSpacing)
          .attr('y', legendRectSize - legendSpacing)
          .text(function (d) { return d.label; });
    }
};

window.AIV.init();