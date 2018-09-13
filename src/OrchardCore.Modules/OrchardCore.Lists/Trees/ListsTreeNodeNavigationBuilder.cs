using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Records;
using OrchardCore.ContentTree.Services;
using OrchardCore.Environment.Navigation;
using YesSql;

namespace OrchardCore.Lists.Trees
{
    public class ListsTreeNodeNavigationBuilder : ITreeNodeNavigationBuilder
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IContentManager _contentManager;
        private readonly ISession _session;

        public ListsTreeNodeNavigationBuilder(
            IContentDefinitionManager contentDefinitionManager,
            IContentManager contentManager,
            ISession session)
        {
            _contentDefinitionManager = contentDefinitionManager;
            _contentManager = contentManager;
            _session = session;        
        }

        public string Name => typeof(ListsTreeNode).Name;

        public void BuildNavigation(MenuItem menuItem, NavigationBuilder builder, IEnumerable<ITreeNodeNavigationBuilder> treeNodeBuilders)
        {
            var tn = menuItem as ListsTreeNode;

            if (tn == null)
            {
                return;
            }

            var contentTypeDefinitions = _contentDefinitionManager.ListTypeDefinitions().OrderBy(d => d.Name);

            //var listable = contentTypeDefinitions.Where(ctd => ctd.Settings.ToObject<ContentTypeSettings>().Listable).OrderBy(ctd => ctd.DisplayName);
            var selected = contentTypeDefinitions
                .Where(ctd => tn.ContentTypes.ToList<string>().Contains(ctd.Name))
                .Where(ctd => ctd.DisplayName != null);

            foreach (var ctd in selected)
            {
                builder.Add(new LocalizedString(ctd.DisplayName, ctd.DisplayName), async listTypeMenu =>
               {
                   //listTypeMenu.Url("/lkjdlsa");

                   var ListContentItems = await _session.Query<ContentItem, ContentItemIndex>()
                       .With<ContentItemIndex>(x => x.Latest)
                       .With<ContentItemIndex>(x => x.ContentType == ctd.Name)
                       .ListAsync();

                   foreach (var ci in ListContentItems)
                   {
                       var cim = await _contentManager.PopulateAspectAsync<ContentItemMetadata>(ci);

                       if ((cim.AdminRouteValues.Any()) && (cim.DisplayText != null))
                       {
                           listTypeMenu.Add(new LocalizedString(cim.DisplayText, cim.DisplayText), m => m
                           .Action(cim.AdminRouteValues["Action"] as string, cim.AdminRouteValues["Controller"] as string, cim.AdminRouteValues)
                           .LocalNav());
                       }
                   }
               });
            }


        }
    }
}



//using System;
//using System.Collections.Generic;
//using System.Text;
//using Microsoft.AspNetCore.Routing;
//using Microsoft.Extensions.Localization;
//using OrchardCore.ContentManagement.Metadata;
//using OrchardCore.ContentTree.Models;
//using OrchardCore.ContentTree.Services;
//using OrchardCore.ContentTree.Trees;
//using OrchardCore.Environment.Navigation;
//using System.Linq;
//using OrchardCore.ContentManagement.Metadata.Settings;
//using OrchardCore.Contents;
//using YesSql;
//using OrchardCore.ContentManagement;
//using OrchardCore.ContentManagement.Records;

//namespace OrchardCore.Lists.Trees
//{
//    public class ListsTreeNodeNavigationBuilder : ITreeNodeNavigationBuilder
//    {
//        private readonly IContentDefinitionManager _contentDefinitionManager;
//        private readonly IContentManager _contentManager;
//        private readonly ISession _session;
//        //private readonly IEnumerable<ITreeNodeNavigationBuilder> _treeNodeNavigationBuilders;

//        public ListsTreeNodeNavigationBuilder(
//            IContentDefinitionManager contentDefinitionManager,
//            IContentManager contentManager,
//            ISession session,
//            IStringLocalizer<ListsTreeNodeNavigationBuilder> localizer)
//        {
//            _contentDefinitionManager = contentDefinitionManager;
//            _contentManager = contentManager;
//            _session = session;
//            //_treeNodeNavigationBuilders = treeNodeNavigationBuilders;
//            T = localizer;
//        }

//        public string Name => typeof(ListsTreeNode).Name;
//        public IStringLocalizer T { get; set; }

//        public void BuildNavigation(MenuItem menuItem, NavigationBuilder builder, IEnumerable<ITreeNodeNavigationBuilder> treeNodeBuilders)
//        {
//            var tn = menuItem as ListsTreeNode;

//            if (tn == null)
//            {
//                return;
//            }

//            var contentTypeDefinitions = _contentDefinitionManager.ListTypeDefinitions().OrderBy(d => d.Name);

//            //var listable = contentTypeDefinitions.Where(ctd => ctd.Settings.ToObject<ContentTypeSettings>().Listable).OrderBy(ctd => ctd.DisplayName);
//            var selected = contentTypeDefinitions.Where(ctd => tn.ContentTypes.ToList<string>().Contains(ctd.Name));


//            builder.Add(T["Content"], "1.4", content =>
//            {
//                content.AddClass("content").Id("content")
//               .Add(T["Lists"], "1", contentItems =>
//               {
//                   contentItems
//                   .LinkToFirstChild(false)
//                   .Permission(Permissions.EditOwnContent)
//                   .Action("List", "Admin", new { area = "OrchardCore.Contents" });

//                   foreach (var ctd in selected)
//                   {
//                       contentItems.Add(new LocalizedString(ctd.DisplayName, ctd.DisplayName), async listTypeMenu =>
//                       {
//                           var ListContentItems = await _session.Query<ContentItem, ContentItemIndex>()
//                               .With<ContentItemIndex>(x => x.Latest)
//                               .With<ContentItemIndex>(x => x.ContentType == ctd.Name)
//                               .ListAsync();

//                           foreach (var ci in ListContentItems)
//                           {
//                               var cim = await _contentManager.PopulateAspectAsync<ContentItemMetadata>(ci);

//                               if (cim.AdminRouteValues.Any())
//                               {
//                                   listTypeMenu.Add(new LocalizedString(cim.DisplayText, cim.DisplayText), m => m
//                                   .Action(cim.AdminRouteValues["Action"] as string, cim.AdminRouteValues["Controller"] as string, cim.AdminRouteValues)
//                                   .LocalNav());
//                               }
//                           }
//                       });
//                   }

//                   contentItems.Add(new LocalizedString("childs of list", "childs of list"), children =>
//                   {
//                       foreach (var childTreeNode in menuItem.Items)
//                       {
//                           var treeBuilder = treeNodeBuilders.Where(x => x.Name == childTreeNode.ItemType).FirstOrDefault();
//                           treeBuilder.BuildNavigation(childTreeNode, builder, treeNodeBuilders);
//                       }

//                   });
//               });
//            });
//        }
//    }
//}