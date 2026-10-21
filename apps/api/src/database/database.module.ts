import { Inject, Module, OnApplicationShutdown } from '@nestjs/common';
import {
  type Database,
  DATABASE,
  databaseProvider,
} from './database.provider.js';

@Module({
  providers: [databaseProvider],
  exports: [databaseProvider],
})
export class DatabaseModule implements OnApplicationShutdown {
  constructor(@Inject(DATABASE) private readonly db: Database) {}

  async onApplicationShutdown() {
    await this.db.$client.end();
  }
}
