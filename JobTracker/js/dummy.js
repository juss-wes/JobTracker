//This file added to ensure when dockerizing release builds (ex, during publish) the /app/js folder is created.
//You can remove this file if you add other static content; make sure to right click on any newly created
//files and under properties set the Copy to Output Directory property to Copy if Newer or Copy Always... otherwise
//your build output will be missing the file!