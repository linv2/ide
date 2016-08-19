$.get("/api/load", {}, function (rsp) {
    var solutionNode = new treeNode({ title: "解决方案'" + rsp.SolutionName + "'(" + rsp.ProjectProperties.length + "个项目)", fileType: "sln", nodeType: "solution" });
    for (var i = 0; i < rsp.ProjectProperties.length; i++) {
        var projectProperty = rsp.ProjectProperties[i];
        var projectNode = new treeNode({ title: projectProperty.ProjectName, fileType: "csproj", pathDepth: 2, nodeType: "project" });

        var referenceNode = new treeNode({ title: "引用", pathDepth: 3, nodeType: "reference" });
        if (projectProperty.ProjectReference && projectProperty.ProjectReference.length > 0) {
            $(projectProperty.ProjectReference).each(function () {
                var referenceItemNode = new treeNode({ title: this, pathDepth: 4, nodeType: "referenceItem" });
                referenceNode.append(referenceItemNode, false);
            });
        }
        projectNode.append(referenceNode, false);


        createFileAndFolderNode(projectProperty.ProjectFiles, projectNode, 3);


        solutionNode.append(projectNode);
    }
    $("ul.tree").append(solutionNode.Node);
    $("ul.tree div.tree-node").click(function () {

        $(".tree-node-selected").removeClass("tree-node-selected");
        $(this).addClass("tree-node-selected");
    });
}, "json");


function createFileAndFolderNode(fileNodeObj, pNode, Depth) {
    if (fileNodeObj) {
        if (fileNodeObj.Folders)
            $(fileNodeObj.Folders).each(function () {
                var _folderNode = new treeNode({ title: this.FolderName, pathDepth: Depth, nodeType: "folder" });
                createFileAndFolderNode(this, _folderNode, Depth + 1);
                pNode.append(_folderNode);
            });
        if (fileNodeObj.Files)
            $(fileNodeObj.Files).each(function () {
                var _fileNode = new treeNode({ title: this.FileName, pathDepth: Depth, nodeType: "file", fileType: this.FileType, tag: this });
                pNode.append(_fileNode);
            });
    }
}



var treeNode = function (options) {
    var defaults = {
        nodeType: "file",//节点类型
        fileType: "",//文件类型
        pathDepth: 1,//深度
        title: "默认节点",//显示的名字
        iconSize: 18,
        tag: {}
    };
    options = $.extend({}, defaults, options);


    this.nodeType = options.nodeType;
    this.fileType = options.fileType;
    this.tag = options.tag;


    var tempindent = '';
    for (var i = 0; i < options.pathDepth; i++) {
        tempindent += '<span class="tree-indent"></span>';
    }

    function getIconHtml() {
        if (options.nodeType && options.nodeType == "folder") {
            return '<span class="tree-icon tree-folder"></span>';
        } else {
            return '<span class="tree-icon"><img src="/images/icons/' + options.iconSize + '/' + options.fileType + '.png" /></span>';
        }
    }
    var nodeHtml = '<li><div class="tree-node ">' + tempindent + '<span class="tree-hit tree-collapsed"></span>' +
                                '' + getIconHtml() +
                                '<span class="tree-title">' + options.title + '</span>' +
            '</div></li>';
    var $nodeObj = $(nodeHtml);
    $nodeObj.data("treeObj", this);
    this.Node = $nodeObj;
    this.append = function (_treeNode, isShow) {
        var chindNode = $nodeObj.find(">div").next();
        if (!chindNode.is("ul")) {
            chindNode = $("<ul></ul>").hide();
            $nodeObj.append(chindNode);
        }
        if (isShow) {
            chindNode.show();
        }
        chindNode.append(_treeNode.Node)
        if (_remove) {
            checkShowhit();
        }
        return this;
    }
    this.find = function (selector) {
        return this.Node.find(selector);
    }
    this.hasChild = function () {
        return $nodeObj.find(">div").next().is("ul");
    }
    function RegisterEvent(treeObj) {
        $nodeObj.find(">div span.tree-hit").click(function () {
            if (!$nodeObj.find(">div").next().is("ul")) { return; }
            var $this = $(this);
            if ($this.hasClass("tree-collapsed")) {
                $this.removeClass("tree-collapsed").addClass("tree-expanded");
                $nodeObj.find(">div").next().show();
                if (options.nodeType && options.nodeType == "folder") {
                    $nodeObj.find(">div span.tree-icon").addClass("tree-folder-open");
                }
            } else {
                $this.removeClass("tree-expanded").addClass("tree-collapsed");
                $nodeObj.find(">div").next().hide();
                if (options.nodeType && options.nodeType == "folder") {
                    $nodeObj.find(">div span.tree-icon").removeClass("tree-folder-open");
                }
            }
        });
        $nodeObj.find(">div.tree-node").dblclick(function () {
            var _treeNode = $(this).parent().data("treeObj");
            switch (_treeNode.nodeType) {
                case "solution":
                case "project":
                case "reference":
                case "referenceItem":
                case "folder": {
                    _treeNode.find(">div span.tree-hit").trigger("click");
                } break;
                case "file": {
                    fileNodeEvent(_treeNode);
                } break;
            }
        });
    }

    function fileNodeEvent(treeNodeObj) {
        alert(JSON.stringify(treeNodeObj));
        var content = '<iframe scrolling="auto" frameborder="0"  src="http://www.baidu.com" style="width:100%;height:100%;"></iframe>';
        $('div.easyui-tabs').tabs('add', {
            title: treeNodeObj.tag.FileName,
            content: content,
            closable: true
        });

    }
    var _remove = true;;//如果需要展开的扩展，则需要图标向前移一个像素
    function checkShowhit() {
        if ($nodeObj.find(">div").next().is("ul")) {
            $nodeObj.find(">div span.tree-hit").removeAttr("style");
            if (_remove) {
                _remove = false;
                $nodeObj.find(">div span.tree-indent:first").remove();
            }
        } else {
            $nodeObj.find(">div span.tree-hit").hide();
        }
    }
    checkShowhit();
    RegisterEvent();
};

