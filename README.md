Sitecore-Analytics
==================

Author: Tim Braga
<br/>Email: tim.braga@velir.com
<br/>Twitter: @tbraga01

The analytics library is meant to provide developers and testers with more information and easy access to certain analytic calls.  The Analytics Information Panel is the real driver which provides a front-end panel that displays visitor and engagement information.  It allows developers and testers to see the data they are accruing in the analytics database and verify certain goals and conditions are met as they navigate the website.

Features:

* Multivariant Report
 * Shows how effective each variation is for a specific test and goal.  It does this by showing how many visits saw the variation against how many met the goal selected. 
* Analytics Information Panel
 * Place this user control at the beginning of your form tag and watch your visitor accrue goals and view visit and visitor information.
 * This control also contains three commands that allow you to refresh the panel, create a new visit, and to have DMS commit the data to the database so you don't have to wait for Sitecore's scheduled database write.
* Page Statistics Context
 * Utilizes Sitecore's Tracker and Linq to provide data access.
* Analytics Control
 * Have your sublayouts inherit from the Analytics control and gain access to that component's Datasource, Page Statistics 
and many more helper methods.
* Variant Mapping
 * Allows you to right click an item in the Context Menu and Insert a variation template.  The reason here is they may want to insert a wrapped template so they can identify a variation template.  This mapping also allows you to create variations custom to the template mapping.  For example, some templates you may want the variations to live under the content item, other's in a folder under the content item or even in a global variation directory.