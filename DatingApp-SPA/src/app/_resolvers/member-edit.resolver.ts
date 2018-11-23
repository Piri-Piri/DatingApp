import { Injectable } from '@angular/core';
import { Resolve, Router, ActivatedRouteSnapshot } from '@angular/router';
import { UserService } from '../_services/user.service';
import { User } from '../_models/user';
import { AlertifyService } from '../_services/alertify.service';
import { Observable, of } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { AuthService } from '../_services/auth.service';

@Injectable()
export class MemberEditResolver implements Resolve<User> {
    constructor(private userService: UserService, private authService: AuthService,
        private router: Router, private alertify: AlertifyService) {}

    resolve(route: ActivatedRouteSnapshot): Observable<User> {
        // resolve required data before navigate to member details
        return this.userService.getUser(this.authService.decodedToken.nameid)
            // catch error and go back to members page
            .pipe(catchError(error => {
                this.alertify.error('Problem retrieving your data');
                this.router.navigate(['/members']);
                // return an observable of null
                return of(null);
            })
        );
    }
}
