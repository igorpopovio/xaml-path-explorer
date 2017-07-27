# XAML Path Explorer

A tool that renders all the paths you have in your solution.

It supports the [Path Markup Syntax](https://docs.microsoft.com/en-us/dotnet/framework/wpf/graphics-multimedia/path-markup-syntax) like the following:

    <Path Stroke="Black"
          Fill="Gray"
          Data="M 10,100 C 10,300 300,-200 300,100" />

If they are not named properly (using `x:Name` or other properties) then it's quite difficult to know what the path represents. This tool is meant to alleviate this issue.
