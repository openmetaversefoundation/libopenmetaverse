<?php

//=============================================================================
// System  : Sandcastle Help File Builder (PHP port)
// Author  : Latif Khalifa <latifer@streamgrid.net>, All rights reserved
// Copyright (c) 2011, Latif Khalifa <latifer@streamgrid.net>
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:

// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//
// Based on C# version by Eric Woodruff  (Eric@EWoodruff.us) from code by Ferdinand Prantl
// Copyright 2008, Eric Woodruff, All rights reserved

$ret = "";
$toc = DOMDocument::load("WebTOC.xml");
$navToc = new DOMXpath($toc);
$root = $navToc->query("//HelpTOCNode[@Id='" . $_GET["Id"] . "']/*" );

if ($root->length == 0) {
  print "<b>TOC node not found!</b>";
  die();
}

foreach ($root as $node) {

  if ($node->hasChildNodes()) {
    $id = $node->getAttribute("Id");
    $title = $node->getAttribute("Title");
    $url = $node->getAttribute("Url");

    if ($url) {
      $target = " target=\"TopicContent\"";
    } else {
      $url = "#";
      $target = "";
    }

    $ret .= sprintf("<div class=\"TreeNode\">\r\n" .
		      "<img class=\"TreeNodeImg\" " .
		      "onclick=\"javascript: Toggle(this);\" " .
		      "src=\"Collapsed.gif\"/><a class=\"UnselectedNode\" " .
		      "onclick=\"javascript: return Expand(this);\" " .
		      "href=\"%s\"%s>%s</a>\r\n" .
		      "<div id=\"%s\" class=\"Hidden\"></div>\r\n</div>\r\n",
		      $url, $target, htmlentities($title), $id);
  } else {

    $title = $node->getAttribute("Title");
    $url = $node->getAttribute("Url");

    if (!$url)
      $url = "about:blank";

    $ret .= sprintf("<div class=\"TreeItem\">\r\n" .
		      "<img src=\"Item.gif\"/>" .
		      "<a class=\"UnselectedNode\" " .
		      "onclick=\"javascript: return SelectNode(this);\" " .
		      "href=\"%s\" target=\"TopicContent\">%s</a>\r\n" .
		      "</div>\r\n", 
		      $url, htmlentities($title));

  }

}

print $ret;