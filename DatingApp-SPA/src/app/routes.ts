import { Routes } from '@angular/router';
import { HomeComponent } from './home/home.component';
import { MemberListComponent } from './members/member-list/member-list.component';
import { ListsComponent } from './lists/lists.component';
import { MessagesComponent } from './messages/messages.component';
import { AuthGuard } from './_guards/auth.guard';
import { MemberListResolver } from './_resolvers/member-list.resolver';
import { MemberDetailResolver } from './_resolvers/member-detail.resolver';
import { MemberDetailComponent } from './members/member-detail/member-detail.component';


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
            { path: 'messages', component: MessagesComponent },
            { path: 'lists', component: ListsComponent },
        ]
    },
    // all other path are redirected to home
    { path: '**', redirectTo: '', pathMatch: 'full'}
];
