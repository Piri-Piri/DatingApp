import { Routes } from '@angular/router';
import { HomeComponent } from './home/home.component';
import { MemberListComponent } from './member-list/member-list.component';
import { ListsComponent } from './lists/lists.component';
import { MessagesComponent } from './messages/messages.component';
import { AuthGuard } from './_guards/auth.guard';

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
            { path: 'members', component: MemberListComponent },
            { path: 'messages', component: MessagesComponent },
            { path: 'lists', component: ListsComponent },
        ]
    },
    // all other path are redirected to home
    { path: '**', redirectTo: '', pathMatch: 'full'}
];
