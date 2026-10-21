import { Test, TestingModule } from '@nestjs/testing';
import type { Lodge } from '../database/schema.js';
import { LodgesController } from './lodges.controller.js';
import { LodgesService } from './lodges.service.js';

describe('LodgesController', () => {
  let lodgesController: LodgesController;
  const lodgesService = { findAll: vi.fn() };

  beforeEach(async () => {
    const app: TestingModule = await Test.createTestingModule({
      controllers: [LodgesController],
      providers: [{ provide: LodgesService, useValue: lodgesService }],
    }).compile();

    lodgesController = app.get<LodgesController>(LodgesController);
  });

  describe('findAll', () => {
    it('should return all lodges', async () => {
      const result: Lodge[] = [
        { id: '7d3f1c2e-5b4a-4e8f-9c1d-2a3b4c5d6e7f', name: 'Test Lodge' },
      ];
      lodgesService.findAll.mockResolvedValue(result);

      await expect(lodgesController.findAll()).resolves.toEqual(result);
    });
  });
});
