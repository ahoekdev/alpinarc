import { Test, TestingModule } from '@nestjs/testing';
import type { Lodge } from '../database/schema.js';
import { LodgesController } from './lodges.controller.js';
import { LodgesService } from './lodges.service.js';

describe('LodgesController', () => {
  let lodgesController: LodgesController;
  const lodgesService = {
    findAll: vi.fn(),
    findOne: vi.fn(),
    create: vi.fn(),
  };
  const lodge: Lodge = {
    id: '7d3f1c2e-5b4a-4e8f-9c1d-2a3b4c5d6e7f',
    name: 'Test Lodge',
  };

  beforeEach(async () => {
    const app: TestingModule = await Test.createTestingModule({
      controllers: [LodgesController],
      providers: [{ provide: LodgesService, useValue: lodgesService }],
    }).compile();

    lodgesController = app.get<LodgesController>(LodgesController);
  });

  describe('findAll', () => {
    it('should return all lodges', async () => {
      lodgesService.findAll.mockResolvedValue([lodge]);

      await expect(lodgesController.findAll()).resolves.toEqual([lodge]);
    });
  });

  describe('findOne', () => {
    it('should return the lodge with the given id', async () => {
      lodgesService.findOne.mockResolvedValue(lodge);

      await expect(lodgesController.findOne(lodge.id)).resolves.toEqual(lodge);
      expect(lodgesService.findOne).toHaveBeenCalledWith(lodge.id);
    });
  });

  describe('create', () => {
    it('should create and return a lodge', async () => {
      lodgesService.create.mockResolvedValue(lodge);

      await expect(
        lodgesController.create({ name: lodge.name }),
      ).resolves.toEqual(lodge);
      expect(lodgesService.create).toHaveBeenCalledWith({ name: lodge.name });
    });
  });
});
