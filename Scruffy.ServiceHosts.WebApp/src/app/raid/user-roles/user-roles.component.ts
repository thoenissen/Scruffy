import { Component, Inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { ChartConfiguration } from 'chart.js';

@Component({
  selector: 'app-user-roles',
  templateUrl: './user-roles.component.html',
  styleUrls: ['./user-roles.component.scss'],
})
export class UserRolesComponent {
  public constants: Constants = Constants.current;

  // Table
  public rows: RaidUserTableRow[] = [];
  public displayedColumns: string[] = [
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

  // Chart
  public barChartLegend: boolean = false;
  public barChartData: ChartConfiguration<'bar'>['data'] | undefined;
  public barChartOptions: ChartConfiguration<'bar'>['options'] = {
    responsive: false,
  };

  constructor(http: HttpClient, @Inject('WEBAPI_BASE_URL') baseUrl: string) {
    http.get<RaidUser[]>(baseUrl + 'raid/users').subscribe(
      (result) => {
        this.prepareResults(result);
      },
      (error) => console.error(error)
    );
  }

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

    this.barChartData = {
      labels: [
        'DPS',
        'Alacrity | DPS',
        'Quickness | DPS',
        'Alacrity | Heal',
        'Quickness | Heal',
        'Tank | Alacrity | DPS',
        'Tank | Quickness | DPS',
        'Tank | Alacrity | Heal',
        'Tank | Quickness | Heal',
      ],
      datasets: [
        {
          data: [
            rows.filter((r) => r.isDamageDealer).length,
            rows.filter((r) => r.isDamageDealerAlacrity).length,
            rows.filter((r) => r.isDamageDealerQuickness).length,
            rows.filter((r) => r.isHealerAlacrity).length,
            rows.filter((r) => r.isHealerQuickness).length,
            rows.filter((r) => r.isTankDamageDealerAlacrity).length,
            rows.filter((r) => r.isTankDamageDealerQuickness).length,
            rows.filter((r) => r.isTankHealerAlacrity).length,
            rows.filter((r) => r.isTankHealerQuickness).length,
          ],
          backgroundColor: 'darkgrey',
          hoverBackgroundColor: 'grey',
          borderColor: 'transparent',
          hoverBorderColor: 'transparent',
        },
      ],
    };

    this.rows = rows;
  }

  getPercentage(roldeId: number) {
    var percentage: number = 0.0;

    if (this.rows.length > 0) {
      switch (roldeId) {
        case this.constants.RaidRoleDamageDealer:
          {
            percentage = this.rows.filter((r) => r.isDamageDealer).length / this.rows.length;
          }
          break;
        case this.constants.RaidRoleDamageDealerAlacrity:
          {
            percentage = this.rows.filter((r) => r.isDamageDealerAlacrity).length / this.rows.length;
          }
          break;
        case this.constants.RaidRoleDamageDealerQuickness:
          {
            percentage = this.rows.filter((r) => r.isDamageDealerQuickness).length / this.rows.length;
          }
          break;
        case this.constants.RaidRoleHealerAlacrity:
          {
            percentage = this.rows.filter((r) => r.isHealerAlacrity).length / this.rows.length;
          }
          break;
        case this.constants.RaidRoleHealerQuickness:
          {
            percentage = this.rows.filter((r) => r.isHealerQuickness).length / this.rows.length;
          }
          break;
        case this.constants.RaidRoleTankDamageDealerAlacrity:
          {
            percentage = this.rows.filter((r) => r.isTankDamageDealerAlacrity).length / this.rows.length;
          }
          break;
        case this.constants.RaidRoleTankDamageDealerQuickness:
          {
            percentage = this.rows.filter((r) => r.isTankDamageDealerQuickness).length / this.rows.length;
          }
          break;
        case this.constants.RaidRoleTankHealerAlacrity:
          {
            percentage = this.rows.filter((r) => r.isTankHealerAlacrity).length / this.rows.length;
          }
          break;
        case this.constants.RaidRoleTankHealerQuickness:
          {
            percentage = this.rows.filter((r) => r.isTankHealerQuickness).length / this.rows.length;
          }
          break;
      }
    }
    return (Math.round(percentage * 10000) / 10000);
  }
}

export class Constants {
  static readonly current: Constants = new Constants();

  readonly RaidRoleDamageDealer: number = 1;
  readonly RaidRoleDamageDealerAlacrity: number = 2;
  readonly RaidRoleDamageDealerQuickness: number = 3;
  readonly RaidRoleHealerAlacrity: number = 4;
  readonly RaidRoleHealerQuickness: number = 5;
  readonly RaidRoleTankDamageDealerAlacrity: number = 6;
  readonly RaidRoleTankDamageDealerQuickness: number = 7;
  readonly RaidRoleTankHealerAlacrity: number = 8;
  readonly RaidRoleTankHealerQuickness: number = 9;
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
