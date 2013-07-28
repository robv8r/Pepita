using System.Collections.Generic;
using System.Runtime.InteropServices;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;

public class CurrentProjectFinder
{
    public List<Project> GetCurrentProjects()
    {
        var dte = (DTE)ServiceProvider.GlobalProvider.GetService(typeof(DTE));
        if (dte.Solution == null)
        {
            return new List<Project>();
        }

        if (string.IsNullOrEmpty(dte.Solution.FullName))
        {
            return new List<Project>();
        }

        var projects = new List<Project>();

        try
        {
            var selectedItems = dte.SelectedItems;
            foreach (SelectedItem selectedItem in selectedItems)
            {
                var projectsOfSelectedItem = GetProjectsForSelectedItem(selectedItem);
                foreach (var project in projectsOfSelectedItem)
                {
                    if (!projects.Contains(project))
                    {
                        projects.Add(project);
                    }
                }
            }
        }
        catch (COMException)
        {
            // Swallow
        }

        return projects;
    }

    private IList<Project> GetProjectsForSelectedItem(SelectedItem selectedItem)
    {
        var projects = new List<Project>();

        if (selectedItem == null)
        {
            return projects;
        }

        if (selectedItem.Project != null)
        {
            if (selectedItem.Project.Kind == ProjectKinds.vsProjectKindSolutionFolder)
            {
                var solutionFolderProjects = GetSolutionFolderProjects(selectedItem.Project);
                projects.AddRange(solutionFolderProjects);
            }
            else
            {
                projects.Add(selectedItem.Project);
            }
        }
        else if (string.Equals(selectedItem.Name, selectedItem.DTE.Solution.Properties.Item("Name").Value))
        {
            var solutionProjects = GetSolutionProjects(selectedItem.DTE.Solution);
            projects.AddRange(solutionProjects);
        }

        return projects;
    }

    private IList<Project> GetSolutionProjects(Solution solution)
    {
        var projects = solution.Projects;
        var list = new List<Project>();
        var item = projects.GetEnumerator();
        while (item.MoveNext())
        {
            var project = item.Current as Project;
            if (project == null)
            {
                continue;
            }

            if (project.Kind == ProjectKinds.vsProjectKindSolutionFolder)
            {
                list.AddRange(GetSolutionFolderProjects(project));
            }
            else if (!ShouldIgnoreProject(project))
            {
                list.Add(project);
            }
        }

        return list;
    }

    private IList<Project> GetSolutionFolderProjects(Project solutionFolder)
    {
        var projects = new List<Project>();
        for (var i = 1; i <= solutionFolder.ProjectItems.Count; i++)
        {
            var subProject = solutionFolder.ProjectItems.Item(i).SubProject;
            if (subProject == null)
            {
                continue;
            }

            // If this is another solution folder, do a recursive call, otherwise add
            if (subProject.Kind == ProjectKinds.vsProjectKindSolutionFolder)
            {
                projects.AddRange(GetSolutionFolderProjects(subProject));
            }
            else if (!ShouldIgnoreProject(subProject))
            {
                projects.Add(subProject);
            }
        }

        return projects;
    }

    private bool ShouldIgnoreProject(Project project)
    {
        if (string.Equals(project.Kind, "{66A26720-8FB5-11D2-AA7E-00C04F688DDE}")) // Solution items
        {
            return true;
        }

        if (string.Equals(project.Kind, "{66A2671D-8FB5-11D2-AA7E-00C04F688DDE}")) // Misc items
        {
            return true;
        }

        return false;
    }
}