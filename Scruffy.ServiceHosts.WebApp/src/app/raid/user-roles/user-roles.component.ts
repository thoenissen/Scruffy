import { Component, OnInit, Inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';

@Component({
  selector: 'app-user-roles',
  templateUrl: './user-roles.component.html',
  styleUrls: ['./user-roles.component.scss'],
})
export class UserRolesComponent implements OnInit {
  rows: RaidUserTableRow[] = [];
  displayedColumns: string[] = [
    'name',
    'dps',
    'dps-alacrity',
    'dps-quickness',
    'healer-alacrity',
    'healer-quickness',
    'tank-dps-alacrity',
    'tank-dps-quickness',
    'tank-healer-alacrity',
    'tank-healer-quickness',
  ];

  constructor(http: HttpClient, @Inject('BASE_URL') baseUrl: string) {
    http.get<RaidUser[]>('api/raid/users').subscribe(
      (result) => {
        this.prepareResults(result);
      },
      (error) => console.error(error)
    );
  }

  ngOnInit(): void {}

  prepareResults(result: RaidUser[]) {
    var rows: RaidUserTableRow[] = [];

    result.forEach(function (user) {
      var currentRow: RaidUserTableRow = {
        name: user.name,
        isDamageDealer: user.assignedRoles.includes(1),
        isDamageDealerAlacrity: user.assignedRoles.includes(2),
        isDamageDealerQuickness: user.assignedRoles.includes(3),
        isHealerAlacrity: user.assignedRoles.includes(4),
        isHealerQuickness: user.assignedRoles.includes(5),
        isTankDamageDealerAlacrity: user.assignedRoles.includes(6),
        isTankDamageDealerQuickness: user.assignedRoles.includes(7),
        isTankHealerAlacrity: user.assignedRoles.includes(8),
        isTankHealerQuickness: user.assignedRoles.includes(9),
      };

      rows.push(currentRow);
    });

    this.rows = rows;
  }
}

export interface RaidUser {
  id: number;
  name: string;
  assignedRoles: number[];
}

export interface RaidUserTableRow {
  name: string;
  isDamageDealer: boolean;
  isDamageDealerAlacrity: boolean;
  isDamageDealerQuickness: boolean;
  isHealerAlacrity: boolean;
  isHealerQuickness: boolean;
  isTankDamageDealerAlacrity: boolean;
  isTankDamageDealerQuickness: boolean;
  isTankHealerAlacrity: boolean;
  isTankHealerQuickness: boolean;
}
