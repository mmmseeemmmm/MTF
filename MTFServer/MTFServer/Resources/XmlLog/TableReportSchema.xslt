<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl"
>

  <xsl:variable name="yellowColor" select="'#FEC100'" />
  <xsl:variable name="gsColor" select="'#FEC100'" />
  <xsl:variable name="howerColor" select="'#ffffff'" />
  <xsl:variable name="okColor" select="'#B7E289'" />
  <xsl:variable name="nokColor" select="'#FC7E7E'" />
  <xsl:variable name="evenColor" select="'#f2f2f2'" />
  <xsl:variable name="oddColor" select="'#fbfbfb'" />
  <xsl:variable name="notFilledColor" select="'#C9C9C9'" />
  <xsl:variable name="tablesCount" select="count(TableLog/Tables/Table)" />
  <xsl:variable name="errorImagesCount" select="count(TableLog/ErrorImages/ErrorImage)" />
  <xsl:variable name="errorsCount" select="count(TableLog/Errors/Error)" />
  <xsl:variable name="messagesCount" select="count(TableLog/Messages/Message)" />
  <xsl:variable name="sequenceStatus" select="TableLog/Header/SequenceStatus" />
  <xsl:variable name="duration" select="TableLog/Header/Duration" />

  <xsl:template name="dateTimeConverter">
    <xsl:param name="dateTime" />
    <xsl:value-of select="concat(
                          substring($dateTime, 1, 4),
                          '-',
                          substring($dateTime, 6, 2),
                          '-',
                          substring($dateTime, 9, 2),
                          ' ',
                          substring($dateTime, 12, 8)
                      )"/>
  </xsl:template>

  <xsl:template name="durationConverter">
    <xsl:param name="dur" />
    <xsl:variable name="first" select="substring-before($dur, '.')" />
    <xsl:variable name="second" select="substring-after($dur, '.')" />
    <xsl:choose>
      <xsl:when test="contains($dur,'.')">
        <xsl:choose>
          <xsl:when test="contains($second,'.') or contains($second,':')">
            <xsl:choose>
              <xsl:when test="contains($second,'.')">
                <xsl:value-of select="concat($first, 'd ', substring-before($second,'.'))"/>
              </xsl:when>
              <xsl:otherwise>
                <xsl:value-of select="concat($first, 'd ', $second)"/>
              </xsl:otherwise>
            </xsl:choose>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="$first"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="$dur"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:output method="html" doctype-system="http://www.w3.org/TR/html4/strict.dtd"
       doctype-public="-//W3C//DTD HTML 4.01//EN" indent="yes"/>
  <xsl:template match="/">
    <html>
      <head>
        <title>
          MTF table report: <xsl:value-of select="TableLog/Header/SequenceName"/>
        </title>
        <script>
          <![CDATA[
          function showImage(img) {
            document.getElementById("showImageDiv").style.display = "block";
            document.getElementById("showImgeDivImg").src = img;
          }
          function toggleTableVisibility(table)
          {
            var rows = table.getElementsByTagName("tr");
            for (var i = 1; i < rows.length; i++) {
              if (rows[i].style.display == "none"){
                rows[i].style.display = ""
              }
              else{
                rows[i].style.display = "none"
              }
            }         
          }
          ]]>
        </script>
        <Style type="text/css">
          body {
          margin:0px;
          }

          p, table {
          font-family: Avenir, Arial, sans-serif;
          }

          #main {
          width: auto;
          margin-left: 100px;
          margin-right: 100px;
          margin-top: 0px
          }

          #banner {
          background: <xsl:value-of select="$yellowColor"/>;
          height: 15px;
          }

          #header {
          margin-left: auto;
          margin-right: auto;
          text-align: center;
          }

          .logo {
          width: 250px;
          margin-left: auto;
          margin-right: auto;
          margin-top: 50px;
          margin-bottom: 0px;
          }

          .logo img {
          width: 100%
          }

          p.category {
          font-family: Avenir, Arial, sans-serif;
          font-size: 1.1em;
          margin-top: 50px;
          margin-bottom: 0px;
          text-align: center;
          }

          h1 {
          font-family: Avenir, Arial, sans-serif;
          margin-top: 0px;
          margin-bottom: 10px;
          }

          #sequenceInfo {
          display: table;
          margin-left: auto;
          margin-right: auto;
          font-family: Avenir, Arial, sans-serif;
          }

          .sequenceInfoRow  {
          display: table-row;
          }

          .sequenceInfoLeft, .sequenceInfoRight {
          display: table-cell;
          text-align: left;
          margin-left: 100px;
          }


          .sequenceHeaderImg {
          margin: 0px 0px 0px 0px;
          height: 40px;
          }

          .headerTitle {
          font-size: 0.8em;
          font-style: italic;
          text-align: left;
          margin: 0;
          }

          .headerValue {
          font-size: 1em;
          font-weight: bold;
          text-align: left;
          margin: 0;
          width: 160px;
          }

          .headerValueWarning {
          font-size: 1em;
          font-weight: bold;
          text-align: left;
          margin: 0;
          width: 160px;
          color: <xsl:value-of select="$yellowColor"/>;
          }

          #sequenceStatus {
          margin-left: auto;
          margin-right: auto;
          margin-top: 30px;
          margin-bottom: 50px;
          width: 250px;
          }

          p.sequenceStatusHeader {
          border-style: hidden;
          padding: 10px 50px 10px 50px;
          <xsl:choose>
            <xsl:when test="$sequenceStatus = 'Pass'">
              background: <xsl:value-of select="$okColor"/>;
            </xsl:when>
            <xsl:when test="$sequenceStatus = 'Fail'">
              background: <xsl:value-of select="$nokColor"/>;
            </xsl:when>
            <xsl:otherwise>
              background: <xsl:value-of select="$notFilledColor"/>;
            </xsl:otherwise>
          </xsl:choose>
          margin-bottom: 0;
          height: 15px;
          }

          p.sequenceStatusValue {
          border-style: hidden;
          padding: 20px 50px 20px 50px;
          opacity: 0.6;
          <xsl:choose>
            <xsl:when test="$sequenceStatus = 'Pass'">
              background: <xsl:value-of select="$okColor"/>;
            </xsl:when>
            <xsl:when test="$sequenceStatus = 'Fail'">
              background: <xsl:value-of select="$nokColor"/>;
            </xsl:when>
            <xsl:otherwise>
              background: <xsl:value-of select="$notFilledColor"/>;
            </xsl:otherwise>
          </xsl:choose>
          height: 45px;
          font-size: 2.5em;
          font-weight: bold;
          color: white;
          margin: 0;
          }


          .tableName {
          margin: 0;
          }


          .tableContent {
          width: 100%;
          margin-top: 10px;
          }



          .tableContent td, .tableContent th, #tableErrors td, #tableErrors th, #tableMessages td, #tableMessages th {
          padding: 7px;
          text-align: center;
          border: 1px solid white;
          max-width: 200px;
          word-wrap:break-word;
          }

          #tableMessages th:first-child { width: 33% ;}

          .tableHeader {
          margin-top: 0px;
          }

          .tableHeader td {
          padding-right: 7px;
          }

          .tableContent tr:nth-child(even){background-color: <xsl:value-of select="$evenColor"/>}
          .tableContent tr:nth-child(2n+3){background-color: <xsl:value-of select="$oddColor"/>}
          .tableContent tr:hover{background-color: <xsl:value-of select="$howerColor"/>;}

          .tableContentHeaderOk {
          background: <xsl:value-of select="$okColor"/>;
          }

          .tableContentHeaderNok {
          background: <xsl:value-of select="$nokColor"/>;
          }

          .tableContentHeaderNotFilled {
          background: <xsl:value-of select="$notFilledColor"/>;
          }

          .tableContentHeaderGs {
          background: <xsl:value-of select="$gsColor"/>;
          }

          .notOkValue {
          color: red;
          font-weight: bold;
          }


          #tableErrors, #tableMessages {
          width: 100%;
          margin-top: 10px;
          }

          #tableErrors tr:nth-child(even){background-color: <xsl:value-of select="$evenColor"/>}
          #tableErrors tr:nth-child(2n+3){background-color: <xsl:value-of select="$oddColor"/>}
          #tableErrors tr:hover{background-color: <xsl:value-of select="$howerColor"/>;}

          #tableMessages tr:nth-child(even){background-color: <xsl:value-of select="$evenColor"/>}
          #tableMessages tr:nth-child(2n+3){background-color: <xsl:value-of select="$oddColor"/>}
          #tableMessages tr:hover{background-color: <xsl:value-of select="$howerColor"/>;}

          .errorHeader, .messagesHeader {
          background: <xsl:value-of select="$notFilledColor"/>;
          }

          .messagesTimeStamp{
          max-width: 100px;
          }

          #footer {
          text-align: center;
          padding: 50px;
          }

          #footer img {
          width: 120px;
          }

          .tableImage {
          text-align: left;
          margin-top: 50px;
          margin-bottom: 0px;
          height: 50px;
          }

          .imagePreview {
          height: 25px;
          }

          #sequenceInfoTable td {
          padding-bottom: 10px;
          padding-right: 5px;
          }

          .rightHeaderColumn {
          padding-left: 30px;
          }


          .tooltip {
          position: relative;
          display: inline-block;
          }

          .tooltip .tooltiptext {
          visibility: hidden;
          width: 120px;
          background-color: #555;
          color: #fff;
          text-align: center;
          border-radius: 6px;
          padding: 5px 0;
          position: absolute;
          z-index: 1;
          bottom: 125%;
          left: 50%;
          margin-left: -60px;
          opacity: 0;
          transition: opacity 1s;
          }

          .tooltip .tooltiptext::after {
          content: "";
          position: absolute;
          top: 100%;
          left: 50%;
          margin-left: -5px;
          border-width: 5px;
          border-style: solid;
          border-color: #555 transparent transparent transparent;
          }

          .tooltip:hover .tooltiptext {
          visibility: visible;
          opacity: 1;
          }

          .errorImages{
          width: auto;
          margin-top: 50px;
          margin-left: auto;
          margin-right: auto;

          }

          .errorImageContainer{
          width: 250px;
          margin-bottom: 50px;
          margin-left: auto;
          margin-right: auto;
          }

          .errorImageContainer img{
          width: 100%;
          height: auto;
          }

          .errorImagesCellMiddle{
          padding-left: 20px;
          padding-right: 20px;
          }

          .errorImagesCellBorder{
          padding: 0px;
          }

          .errorImageTable{
          margin-left: auto;
          margin-right: auto;
          }



        </Style>
      </head>

      <body>
        <div id="showImageDiv" style=" display:none; z-index:5000; width: 100%; position: fixed; min-height: 100%; background-color:rgba(0, 0, 0, 0.8); overflow-y: auto; bottom: 0; left: 0;" onclick='document.getElementById("showImageDiv").style.display = "none"'>
          <img id="showImgeDivImg" src="" style=" margin:auto; max-width:90%; max-height:90%; position:absolute; top:0;bottom:0; left:0;right:0;" />
        </div>
        <div id="main">
          <div id="banner">
          </div>

          <div id="header">
            <div class="logo">
              <img src="Resources\ALLogo.svg" alt="ALLogo" />
            </div>

            <p class="category">
              Report of sequence:
            </p>
            <h1>
              <xsl:value-of select="TableLog/Header/SequenceName"/>
            </h1>

            <a>
              <xsl:attribute name="href">
                <xsl:value-of select="TableLog/Header/ZipDestination"/>
              </xsl:attribute>
              <img  src="Resources\DownLog.svg" alt="Download" title="Download" />
            </a>

            <div id="sequenceInfo">
              <table id="sequenceInfoTable">
                <tr>
                  <td>
                    <img class="sequenceHeaderImg" src="Resources\machine.svg" alt="Machine" />
                  </td>
                  <td>
                    <p class="headerTitle">Machine:</p>
                    <p class="headerValue">
                      <xsl:value-of select="TableLog/Header/Machine"/>
                    </p>
                  </td>
                  <td class="rightHeaderColumn">
                    <img class="sequenceHeaderImg" src="Resources\startTime.svg" alt="StartTime" />
                  </td>
                  <td>
                    <p class="headerTitle">Start time:</p>
                    <p class="headerValue">
                      <xsl:call-template name="dateTimeConverter">
                        <xsl:with-param name="dateTime" select="TableLog/Header/StartTime" />
                      </xsl:call-template>
                    </p>
                  </td>
                </tr>

                <tr>
                  <td>
                    <img class="sequenceHeaderImg" src="Resources\user.svg" alt="User" />
                  </td>
                  <td>
                    <p class="headerTitle">User:</p>
                    <p class="headerValue">
                      <xsl:value-of select="TableLog/Header/WinUser"/>
                    </p>
                  </td>
                  <td class="rightHeaderColumn">
                    <img class="sequenceHeaderImg" src="Resources\stopTime.svg" alt="StopTime" />
                  </td>
                  <td>
                    <p class="headerTitle">Stop time:</p>
                    <p class="headerValue">
                      <xsl:call-template name="dateTimeConverter">
                        <xsl:with-param name="dateTime" select="TableLog/Header/StopTime" />
                      </xsl:call-template>
                    </p>
                  </td>
                </tr>

                <tr>
                  <td>
                    <img class="sequenceHeaderImg" src="Resources\errors.svg" alt="Errors" />
                  </td>
                  <td>
                    <p class="headerTitle">Errors count:</p>
                    <p class="headerValue">
                      <xsl:value-of select="$errorsCount"/>
                    </p>
                  </td>
                  <td class="rightHeaderColumn">
                    <img class="sequenceHeaderImg" src="Resources\duration.svg" alt="Duration" />
                  </td>
                  <td>
                    <p class="headerTitle">Duration:</p>
                    <p class="headerValue">
                      <xsl:call-template name="durationConverter">
                        <xsl:with-param name="dur" select="$duration" />
                      </xsl:call-template>
                    </p>
                  </td>
                </tr>
                <tr>
                  <td>
                    <img class="sequenceHeaderImg" src="Resources\goldSample.svg" alt="GS" />
                  </td>
                  <td>
                    <p class="headerTitle">Sequence Variant:</p>
                    <p class="headerValue">
                      <xsl:value-of select="TableLog/Header/SequenceVariant"/>
                    </p>
                  </td>
                  <td class="rightHeaderColumn">
                    <img class="sequenceHeaderImg" src="Resources\hourglass.svg" alt="Hourglass" />
                  </td>
                  <td>
                    <p class="headerTitle">Gold Sample life:</p>
                    <p>
                      <xsl:attribute name="class">
                        <xsl:choose>
                          <xsl:when test="TableLog/Header/GsWarning = 'True' or TableLog/Header/GsWarning = 'true'">
                            headerValueWarning
                          </xsl:when>
                          <xsl:otherwise>
                            headerValue
                          </xsl:otherwise>
                        </xsl:choose>
                      </xsl:attribute>
                      <xsl:value-of select="TableLog/Header/GsRemains"/>
                    </p>
                  </td>
                </tr>
              </table>
            </div>

            <div id="sequenceStatus">
              <p class="sequenceStatusHeader">Final result</p>
              <p class="sequenceStatusValue">
                <xsl:value-of select="$sequenceStatus"/>
              </p>
            </div>
          </div>

          <xsl:if test="$errorImagesCount &gt; 0">
            <div class="errorImages">
              <table class="errorImageTable">
                <xsl:for-each select="TableLog/ErrorImages/ErrorImage">
                  <xsl:variable name="position" select="position()" />
                  <xsl:if test="$position mod 3 = 1">
                    <xsl:text disable-output-escaping="yes"><![CDATA[<tr>]]></xsl:text>
                  </xsl:if>
                  <td>
                    <xsl:attribute name="class">
                      <xsl:choose>
                        <xsl:when test="$position mod 3 != 1 and $position mod 3!=0">errorImagesCellMiddle</xsl:when>
                        <xsl:otherwise>errorImagesCellBorder</xsl:otherwise>
                      </xsl:choose>
                    </xsl:attribute>
                    <div class="errorImageContainer">
                      <xsl:attribute name="onClick">
                        showImage("Resources/<xsl:value-of select="text()" />")
                      </xsl:attribute>
                      <img class="errorImage">
                        <xsl:attribute name="src">
                          Resources/<xsl:value-of select="text()" />
                        </xsl:attribute>
                      </img>
                    </div>
                  </td>
                  <xsl:if test="$position mod 3 = 0">
                    <xsl:text disable-output-escaping="yes"><![CDATA[</tr>]]></xsl:text>
                  </xsl:if>
                </xsl:for-each>
              </table>
            </div>
          </xsl:if>

          <xsl:if test="$tablesCount &gt; 0">
            <div id="tables">
              <xsl:for-each select="TableLog/Tables/Table">
                <p class="tableName">
                  <span class="headerValue">
                    <xsl:value-of select="@Name" />
                  </span>
                </p>

                <div style="overflow-x:auto;">
                  <table class="tableContent">
                    <tr>
                      <xsl:attribute name="class">
                        <xsl:choose>
                          <xsl:when test="Status = 'Ok'">
                            tableContentHeaderOk
                          </xsl:when>
                          <xsl:when test="Status = 'Nok'">
                            tableContentHeaderNok
                          </xsl:when>
                          <xsl:when test="Status = 'GSFail'">
                            tableContentHeaderGs
                          </xsl:when>
                          <xsl:otherwise>
                            tableContentHeaderNotFilled
                          </xsl:otherwise>
                        </xsl:choose>
                      </xsl:attribute>
                      <xsl:attribute name="onClick">
                        toggleTableVisibility(event.target.parentElement.parentElement.parentElement);
                      </xsl:attribute>

                      <th>Name</th>
                      <th>Actual value</th>
                      <!--<th>Status</th>-->
                      <!--<th>Repetition</th>-->
                      <xsl:for-each select="Rows/Row[1]/Conditions/Condition">
                        <th>
                          <xsl:value-of select="@Name"/>
                        </th>
                      </xsl:for-each>
                    </tr>
                    <xsl:for-each select="Rows/Row">
                      <xsl:variable name="hasImage" select="HasImage" />
                      <xsl:variable name="rowStatus" select="Status" />
                      <tr>
                        <td style="text-align: left; padding-left: 20px">
                          <xsl:attribute name="class">
                            <xsl:choose>
                              <xsl:when test="Status = 'Ok'">
                                tableContentHeaderOk
                              </xsl:when>
                              <xsl:when test="Status = 'Nok'">
                                tableContentHeaderNok
                              </xsl:when>
                              <xsl:when test="Status = 'GSFail'">
                                tableContentHeaderGs
                              </xsl:when>
                              <xsl:otherwise>
                                tableContentHeaderNotFilled
                              </xsl:otherwise>
                            </xsl:choose>
                          </xsl:attribute>
                          <xsl:value-of select="@Name"/>
                        </td>
                        <td>

                          <xsl:choose>
                            <xsl:when test="$hasImage = 'True' or $hasImage = 'true'">
                              <xsl:element name="div">
                                <xsl:attribute name="onClick">
                                  showImage("Resources/<xsl:value-of select="ActualValue"/>")
                                </xsl:attribute>
                                <img class="imagePreview" src="Resources/photo.svg" alt="Image" />
                              </xsl:element>
                            </xsl:when>
                            <xsl:otherwise>
                              <xsl:choose>
                                <xsl:when test="RoundedValue">
                                  <div class="tooltip">
                                    <xsl:value-of select="RoundedValue"/>
                                    <span class="tooltiptext">
                                      <xsl:value-of select="ActualValue"/>
                                    </span>
                                  </div>
                                </xsl:when>
                                <xsl:otherwise>
                                  <xsl:value-of select="ActualValue"/>
                                </xsl:otherwise>
                              </xsl:choose>
                            </xsl:otherwise>
                          </xsl:choose>
                        </td>
                        <!--<td>
                          <xsl:value-of select="Status"/>
                        </td>-->
                        <!--<td>
                          <xsl:value-of select="NumberOfRepetition"/>
                        </td>-->
                        <xsl:for-each select="Conditions/Condition">
                          <td>
                            <xsl:attribute name="class">
                              <xsl:if test="Status = 'false'  and $rowStatus != 'NotFilled'">notOkValue</xsl:if>
                            </xsl:attribute>
                            <xsl:choose>
                              <xsl:when test="RoundedValue">
                                <div class="tooltip">
                                  <xsl:value-of select="RoundedValue"/>
                                  <span class="tooltiptext">
                                    <xsl:value-of select="Value"/>
                                  </span>
                                </div>
                              </xsl:when>
                              <xsl:otherwise>
                                <xsl:value-of select="Value"/>
                              </xsl:otherwise>
                            </xsl:choose>
                          </td>
                        </xsl:for-each>
                      </tr>
                    </xsl:for-each>
                  </table>
                </div>
                <br />
                <br />
              </xsl:for-each>

            </div>
          </xsl:if>

          <xsl:if test="$messagesCount &gt; 0">
            <div id="messages">
              <img class="tableImage" src="Resources\message.svg" alt="Messages" />
              <div style="overflow-x:auto;">
                <table id="tableMessages">
                  <tr class="messagesHeader">
                    <xsl:attribute name="onClick">
                      toggleTableVisibility(event.target.parentElement.parentElement.parentElement);
                    </xsl:attribute>
                    <th class="messagesTimeStamp">Timestamp</th>
                    <th>Message</th>
                  </tr>
                  <xsl:for-each select="TableLog/Messages/Message">
                    <tr>
                      <td>
                        <xsl:value-of select="TimeStamp"/>
                      </td>
                      <td>
                        <xsl:value-of select="Message"/>
                      </td>
                    </tr>
                  </xsl:for-each>
                </table>
              </div>
            </div>
          </xsl:if>

          <xsl:if test="$errorsCount &gt; 0">
            <div id="errors">
              <img class="tableImage" src="Resources\errors.svg" alt="Errors" />
              <div style="overflow-x:auto;">
                <table id="tableErrors">
                  <tr class="errorHeader">
                    <th>Timestamp</th>
                    <th>Type</th>
                    <th>Activity name</th>
                    <th>Message</th>
                  </tr>
                  <xsl:for-each select="TableLog/Errors/Error">
                    <tr>
                      <td>
                        <xsl:value-of select="TimeStamp"/>
                      </td>
                      <td>
                        <xsl:value-of select="Type"/>
                      </td>
                      <td>
                        <xsl:value-of select="Activity"/>
                      </td>
                      <td>
                        <xsl:value-of select="Message"/>
                      </td>
                    </tr>
                  </xsl:for-each>
                </table>
              </div>
            </div>
          </xsl:if>

          <div id="footer">
            <img src="Resources\ALLogo.svg" alt="ALLogo" />
          </div>
        </div>
      </body>
    </html>
  </xsl:template>
</xsl:stylesheet>


