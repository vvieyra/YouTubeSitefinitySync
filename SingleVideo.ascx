<%@ Control Language="C#" %>
<%@ Register Assembly="Telerik.Sitefinity" Namespace="Telerik.Sitefinity.DynamicModules.Web.UI.Frontend" TagPrefix="sf" %>
<%@ Register Assembly="Telerik.Sitefinity" Namespace="Telerik.Sitefinity.Web.UI.Fields" TagPrefix="sf" %>
<%@ Register Assembly="Telerik.Sitefinity" Namespace="Telerik.Sitefinity.Web.UI" TagPrefix="sf" %>
<%@ Import Namespace="WebVideoSync.Utilities" %>
<sf:DynamicDetailContainer id="detailContainer" runat="server">
    <LayoutTemplate>
        <div class="sfitemDetails videos">
          <a target="_blank" class='<%# Page.Request.Browser.IsMobileDevice ? "mobile" : "" %>' href='http://www.youtube.com/watch?v=<%# Eval("YouTubeVidId")%>' data-vidId='<%# Eval("YouTubeVidId")%>'>
            <img src='<%# WebVideoSync.Utilities.DataHelper.GetImageUrl(Eval("Thumbnail"), Eval("Title")) %>' />
          </a>
          <script src="//www.youtube.com/iframe_api"></script>
          <script type="text/javascript">
          	// This function is automatically called by the player once it loads
          	var ytplayer;
          	function onPlayerReady(event) {
          		event.target.playVideo();
          	}
          	// The "main method" of this sample. Called when someone clicks "Run".
          	function loadPlayer(vidId) {
          		// The video to load
          		if (vidId)
          			var videoID = vidId;
          		else
          			var videoID = "0yHlhUFWvOM";
          		player = new YT.Player('youTubePlayer', {
          			height: '390',
          			width: '640',
          			videoId: videoID,
          			events: {
          				'onReady': onPlayerReady
          			}
          		});
          	}
          	var olayClickOff = function () {
          		//Stop video
          		$('.player-container').html('<div id="youTubePlayer"/>');
          		//Hide player container
          		$('.player-container').hide(0, function () {
          			//Fade out overlay
          			$('.olay').fadeOut('slow');
          		});
          		//Remove olay click event
          		$('.olay').off("click", olayClickOff);
          	}

          	$(window).load(function () {

          		//center player container on screen.
          		$('.player-container').css({ 'top': ($(window).height() - 390) / 2 + 'px', 'left': ($(window).width() - 640) / 2 + 'px' });
          		//on thumnail click run code below
          		$('.youtube-vids a:not(.mobile), .sfitemDetails.videos a:not(.mobile)').click(function (event) {
          			//VideoId
          			var idForVideo = $(this).data('vidid');
          			//Loads video into player
          			loadPlayer(idForVideo);
          			//Fade in overlay
          			$('.olay').fadeIn('slow', function () {
          				//Show player after overlay loads
          				$('.player-container').show(0, function () {
          					//sets click event for overlay to end movie
          					$('.olay').on('click', olayClickOff);
          				});
          			});

          			event.preventDefault();
          			return false;
          		});
          	});
          </script>
          
          <div class="olay" style="display: none;"></div>
          <div class='player-container'>
            <div id="youTubePlayer"></div>
      	  </div>
        </div>
    </LayoutTemplate>
</sf:DynamicDetailContainer>
<asp:PlaceHolder ID="socialOptionsContainer" runat="server"></asp:PlaceHolder>