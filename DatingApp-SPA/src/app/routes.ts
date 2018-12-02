import { Routes } from '@angular/router';
import { AuthGuard } from './_guards/auth.guard';
import { PreventUnsavedChanges } from './_guards/prevent-unsaved-changes.guard';
import { HomeComponent } from './home/home.component';
import { MemberListComponent } from './members/member-list/member-list.component';
import { MemberListResolver } from './_resolvers/member-list.resolver';
import { ListsComponent } from './lists/lists.component';
import { ListsResolver } from './_resolvers/lists.resolver';
import { MessagesComponent } from './messages/messages.component';
import { MemberDetailComponent } from './members/member-detail/member-detail.component';
import { MemberDetailResolver } from './_resolvers/member-detail.resolver';
import { MemberEditComponent } from './members/member-edit/member-edit.component';
import { MemberEditResolver } from './_resolvers/member-edit.resolver';
import { MessagesResolver } from './_resolvers/messages.resolver';
import { AdminPanelComponent } from './admin/admin-panel/admin-panel.component';

export const appRoutes: Routes = [
    /*
    home will reached by an empty path,
    otherwise browsing http://localhost:4200/ will show an empty 'homepage'
    */
    { path: '', component: HomeComponent },
    // dummy route (empty path) to apply guard to all (child) path
    {
        path: '',
        runGuardsAndResolvers: 'always',
        canActivate: [AuthGuard],
        // Append an empty path to child path will finally be the path to be protected.
        children : [
            { path: 'members',
                component: MemberListComponent,
                resolve: { users: MemberListResolver }  },
            { path: 'members/:id',
                component: MemberDetailComponent,
                resolve: { user: MemberDetailResolver } },
            { path: 'member/edit',
                component: MemberEditComponent,
                resolve: { user: MemberEditResolver },
                canDeactivate: [PreventUnsavedChanges] },
            { path: 'messages',
                component: MessagesComponent,
                resolve: { messages: MessagesResolver } },
            { path: 'lists',
                component: ListsComponent,
                resolve: { users: ListsResolver } },
            { path: 'admin',
                component: AdminPanelComponent,
                data: { roles: ['Admin', 'Moderator']}
            }
        ]
    },
    // all other path are redirected to home
    { path: '**', redirectTo: '', pathMatch: 'full'}
];
