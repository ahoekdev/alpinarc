import { Module } from '@nestjs/common';
import { DatabaseModule } from '../database/database.module.js';
import { LodgesController } from './lodges.controller.js';
import { LodgesService } from './lodges.service.js';

@Module({
  imports: [DatabaseModule],
  controllers: [LodgesController],
  providers: [LodgesService],
})
export class LodgesModule {}
