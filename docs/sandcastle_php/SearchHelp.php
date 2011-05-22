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


$searchText = $_GET["Keywords"];


if (!$searchText) {
  print "<b class=\"PaddedText\">Nothing found</b>";
  die();
}

$sortByTitle = $_GET["SortByTitle"] == "true";

$keywords = preg_split("/\\W+/", $searchText);
$keywordList = array();

for ($i=0; $i<count($keywords); $i++) {
  $checkWord = strtolower($keywords[$i]);
  if (strlen($checkWord) > 1 && !isset($keywordList[$checkWord])) {
      $keywordList[$checkWord] = 1;
  }
}

$keywordList = array_keys($keywordList);

if (!count($keywordList)) {
  print "<b class=\"PaddedText\">No search keywords (min length 2)</b>";
  die();
}

$ki = DOMDocument::load("WebKI.xml");
$xpath = new DOMXpath($ki);
$root = $xpath->query("*" );

$max = 100;
$hits = 0;

$ret = "";

foreach($root as $node) {
  
  $title = $node->getAttribute("Title");
  $url = $node->getAttribute("Url");

  $found = true;

  foreach($keywordList as $word) {
    if (false === stristr($title, $word)) {
      $found = false;
      break;
    }
  }

  if ($found) {
    $hits++;

    $ret .= sprintf("<div class=\"TreeItem\">\r\n<img src=\"Item.gif\"/>" .
		    "<a class=\"UnselectedNode\" target=\"TopicContent\" " .
		    "href=\"%s\" onclick=\"javascript: SelectSearchNode(this);\">" .
		    "%s</a>\r\n</div>\r\n", $url, $title);

  }

  if ($hits > $max) break;
}

$ret .= sprintf("<span id=\"SearchKeywords\" style=\"display: none\">%s</span>", implode(" ", $keywordList));

print $ret;