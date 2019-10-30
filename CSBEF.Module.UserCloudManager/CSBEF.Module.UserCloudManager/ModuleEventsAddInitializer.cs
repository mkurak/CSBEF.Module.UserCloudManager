using CSBEF.Core.Enums;
using CSBEF.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace CSBEF.Module.UserCloudManager
{
    public class ModuleEventsAddInitializer : IModuleEventsAddInitializer
    {
        public ModuleEventsAddInitializer()
        {
        }

        public void Start(IEventService eventService)
        {
            if (eventService == null)
            {
                throw new ArgumentNullException(nameof(eventService));
            }

            #region FmService

            #region Base Actions

            eventService.AddEvent("FmService.First.Before", "UserCloudManager", "FmService", "First", EventTypeEnum.before);
            eventService.AddEvent("FmService.FirstOrDefault.Before", "UserCloudManager", "FmService", "FirstOrDefault", EventTypeEnum.before);
            eventService.AddEvent("FmService.Any.Before", "UserCloudManager", "FmService", "Any", EventTypeEnum.before);
            eventService.AddEvent("FmService.List.Before", "UserCloudManager", "FmService", "List", EventTypeEnum.before);

            eventService.AddEvent("FmService.First.After", "UserCloudManager", "FmService", "First", EventTypeEnum.after);
            eventService.AddEvent("FmService.FirstOrDefault.After", "UserCloudManager", "FmService", "FirstOrDefault", EventTypeEnum.after);
            eventService.AddEvent("FmService.Any.After", "UserCloudManager", "FmService", "Any", EventTypeEnum.after);
            eventService.AddEvent("FmService.List.After", "UserCloudManager", "FmService", "List", EventTypeEnum.after);

            #endregion Base Actions

            #region Service Actions

            eventService.AddEvent("FmService.AddDirectory.Before", "UserCloudManager", "FmService", "AddDirectory", EventTypeEnum.before);
            eventService.AddEvent("FmService.UpdateDirectory.Before", "UserCloudManager", "FmService", "UpdateDirectory", EventTypeEnum.before);
            eventService.AddEvent("FmService.ChangeStatusDirectory.Before", "UserCloudManager", "FmService", "ChangeStatusDirectory", EventTypeEnum.before);
            eventService.AddEvent("FmService.ChangeStatusDirectory.Before", "UserCloudManager", "FmService", "ChangeStatusDirectory", EventTypeEnum.before);
            eventService.AddEvent("FmService.MoveDirectory.Before", "UserCloudManager", "FmService", "MoveDirectory", EventTypeEnum.before);
            eventService.AddEvent("FmService.CopyDirectory.Before", "UserCloudManager", "FmService", "CopyDirectory", EventTypeEnum.before);
            eventService.AddEvent("FmService.ShareDirectory.Before", "UserCloudManager", "FmService", "ShareDirectory", EventTypeEnum.before);
            eventService.AddEvent("FmService.AddFile.Before", "UserCloudManager", "FmService", "AddFile", EventTypeEnum.before);
            eventService.AddEvent("FmService.UpdateFile.Before", "UserCloudManager", "FmService", "UpdateFile", EventTypeEnum.before);
            eventService.AddEvent("FmService.ChangeStatusFile.Before", "UserCloudManager", "FmService", "ChangeStatusFile", EventTypeEnum.before);
            eventService.AddEvent("FmService.MoveFile.Before", "UserCloudManager", "FmService", "MoveFile", EventTypeEnum.before);
            eventService.AddEvent("FmService.CopyFile.Before", "UserCloudManager", "FmService", "CopyFile", EventTypeEnum.before);
            eventService.AddEvent("FmService.ShareFile.Before", "UserCloudManager", "FmService", "ShareFile", EventTypeEnum.before);
            eventService.AddEvent("FmService.FmTreeItems.Before", "UserCloudManager", "FmService", "FmTreeItems", EventTypeEnum.before);
            eventService.AddEvent("FmService.FmSharedFiles.Before", "UserCloudManager", "FmService", "FmSharedFiles", EventTypeEnum.before);

            eventService.AddEvent("FmService.AddDirectory.After", "UserCloudManager", "FmService", "AddDirectory", EventTypeEnum.after);
            eventService.AddEvent("FmService.UpdateDirectory.After", "UserCloudManager", "FmService", "UpdateDirectory", EventTypeEnum.after);
            eventService.AddEvent("FmService.ChangeStatusDirectory.After", "UserCloudManager", "FmService", "ChangeStatusDirectory", EventTypeEnum.after);
            eventService.AddEvent("FmService.ChangeStatusDirectory.After", "UserCloudManager", "FmService", "ChangeStatusDirectory", EventTypeEnum.after);
            eventService.AddEvent("FmService.MoveDirectory.After", "UserCloudManager", "FmService", "MoveDirectory", EventTypeEnum.after);
            eventService.AddEvent("FmService.CopyDirectory.After", "UserCloudManager", "FmService", "CopyDirectory", EventTypeEnum.after);
            eventService.AddEvent("FmService.ShareDirectory.After", "UserCloudManager", "FmService", "ShareDirectory", EventTypeEnum.after);
            eventService.AddEvent("FmService.AddFile.After", "UserCloudManager", "FmService", "AddFile", EventTypeEnum.after);
            eventService.AddEvent("FmService.UpdateFile.After", "UserCloudManager", "FmService", "UpdateFile", EventTypeEnum.after);
            eventService.AddEvent("FmService.ChangeStatusFile.After", "UserCloudManager", "FmService", "ChangeStatusFile", EventTypeEnum.after);
            eventService.AddEvent("FmService.MoveFile.After", "UserCloudManager", "FmService", "MoveFile", EventTypeEnum.after);
            eventService.AddEvent("FmService.CopyFile.After", "UserCloudManager", "FmService", "CopyFile", EventTypeEnum.after);
            eventService.AddEvent("FmService.ShareFile.After", "UserCloudManager", "FmService", "ShareFile", EventTypeEnum.after);
            eventService.AddEvent("FmService.FmTreeItems.After", "UserCloudManager", "FmService", "FmTreeItems", EventTypeEnum.after);
            eventService.AddEvent("FmService.FmSharedFiles.After", "UserCloudManager", "FmService", "FmSharedFiles", EventTypeEnum.after);

            #endregion Service Actions

            #endregion FmService
        }
    }
}
